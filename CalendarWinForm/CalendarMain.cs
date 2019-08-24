﻿using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace CalendarWinForm {
    public partial class Form_Calendar_main : Form
    {

        // variable.                                            
        private ThreadManager tManager;
        private ListBox[] gbox;
        private DataAddForm addForm;
        private SQLiteConnection dbConnect;
        private SQLiteCommand dbCommand;
        private int selectYear;
        private int selectMonth;
        private int selectDay;
        private int calendar_index;
        private int gbox_index;
        private string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\baedi_calendar";
        private string dbFileName = @"\calendar.db";


        // main method.                                         
        public Form_Calendar_main() {
            InitializeComponent();
            gbox = new ListBox[42];
            addForm = new DataAddForm(label_DateTemp, this, false);


            // Database setting. 
            dbConnect = new SQLiteConnection("Data Source=" + path + dbFileName + ";Version=3;");
            addForm.setDbConnect(dbConnect);

            if (!File.Exists(path + dbFileName)) {
                string table = "create table calendarlist (year INT, month INT, day INT, sethour INT, setminute INT, text VARCHAR(21), active BOOLEAN)";
                dbCommand = new SQLiteCommand(table, dbConnect);
                Directory.CreateDirectory(path);
                SQLiteConnection.CreateFile(path + dbFileName);
                MessageBox.Show("New calendar db created.");
                dbConnect.Open();
                dbCommand.ExecuteNonQuery();
                dbConnect.Close();
            }


            // Thread setting. 
            tManager = new ThreadManager(label_Time, dbConnect);


            // Panel setting.                                   
            for (int count = 0; count < gbox.Length; count++) gbox[count] = new ListBox();


            // Current date setting. 
            selectDay = int.Parse(DateTime.Now.ToString("dd"));
            selectYear = int.Parse(DateTime.Now.ToString("yyyy"));
            selectMonth = int.Parse(DateTime.Now.ToString("MM"));

            for (int row = 1, count = 0; row <= 6; row++)
                for (int col = 0; col < 7; col++) { panel_MonthList.Controls.Add(gbox[count], col, row); count++; }

            label_DateTemp.Text = selectYear.ToString() + "." + selectMonth.ToString() + "." + selectDay.ToString();
            label_YearMonth.Text = selectYear.ToString() + "." + selectMonth.ToString("00");
            changeCalendar();
        }



        // calendar diary set Method.                           
        private void settingCalendar()
        {
            int maxDays = int.Parse(DateTime.DaysInMonth(selectYear, selectMonth).ToString());
            int blankCount, tempCt;
            DateTime dOfMonth = new DateTime();

            dOfMonth = dOfMonth.AddYears(selectYear - 1).AddMonths(selectMonth - 1);

            switch (dOfMonth.DayOfWeek.ToString())
            {
                case "Sunday": blankCount = 7; break;
                case "Monday": blankCount = 1; break;
                case "Tuesday": blankCount = 2; break;
                case "Wednesday": blankCount = 3; break;
                case "Thursday": blankCount = 4; break;
                case "Friday": blankCount = 5; break;
                default: blankCount = 6; break;
            }

            tempCt = blankCount - 1;
            dbConnect.Open();

            for (int row = 1, boxCount = 0, dayCount = 1; row <= 6; row = row + 1)
                for (int col = 0; col < 7; col = col + 1)
                {

                    if (blankCount > 0 || dayCount > maxDays)
                    {
                        gbox[boxCount].BackColor = System.Drawing.SystemColors.InactiveCaptionText;
                        gbox[boxCount].TabStop = false;
                        blankCount = blankCount - 1;
                    }

                    else
                    {
                        string[] dateStr = new string[3];
                        string sql;


                        // Calender Panel color setting.
                        if (DateTime.Now.ToString("yyyy-MM") == (selectYear.ToString() + "-" + selectMonth.ToString("00")) && dayCount == int.Parse(DateTime.Now.ToString("dd")))
                            gbox[dayCount + tempCt].BackColor = System.Drawing.Color.FromArgb(192, 255, 255);

                        else if (dOfMonth.DayOfWeek.ToString() == "Sunday")
                            gbox[boxCount].BackColor = System.Drawing.Color.FromArgb(255, 192, 192);

                        else if (dOfMonth.DayOfWeek.ToString() == "Saturday")
                            gbox[boxCount].BackColor = System.Drawing.Color.FromArgb(192, 192, 255);

                        else gbox[boxCount].BackColor = System.Drawing.SystemColors.Window;


                        // database setting. 
                        dateStr = dOfMonth.ToString("yyyy-MM-dd").Split('-');
                        dateStr[1] = int.Parse(dateStr[1]).ToString();
                        dateStr[2] = int.Parse(dateStr[2]).ToString();

                        sql = $"select text from calendarlist where year = {dateStr[0]} AND month = {dateStr[1]} AND day = {dateStr[2]} order by sethour, setminute ASC";
                        dbCommand = new SQLiteCommand(sql, dbConnect);
                        SQLiteDataReader reader = dbCommand.ExecuteReader();
                        gbox[boxCount].Items.Insert(0, dayCount);

                        int moreCount = 0;
                        for (int count = 1; reader.Read(); count = count + 1)
                        {
                            if (count >= 4) moreCount = moreCount + 1;
                            else gbox[boxCount].Items.Insert(count, reader["text"].ToString());
                        }

                        if (moreCount > 0) gbox[boxCount].Items.Insert(4, "(...more " + moreCount + ")");

                        reader.Close();

                        gbox[boxCount].TabStop = true;
                        dayCount = dayCount + 1;
                        dOfMonth = dOfMonth.AddDays(1);
                    }

                    gbox[boxCount].Enabled = false;
                    boxCount = boxCount + 1;

                }

            dbConnect.Close();

            gbox_index = selectDay + tempCt;
            gbox[gbox_index].BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
            //MessageBox.Show("Refresh Database");
        }


        public void changeCalendar() {
            for (int count = 0; count < gbox.Length; count++) gbox[count].Items.Clear();
            settingCalendar();
            button_modifySch.Enabled = false;
            button_deleteSch.Enabled = false;
        }


        // database current day calendar import.                
        public void calendarListRefresh()
        {
            string sql = $"select sethour, setminute, text, active from calendarlist where year = {selectYear} AND month = {selectMonth} AND day = {selectDay} order by sethour, setminute ASC;";
            dbConnect.Open();
            listView_Schedule.Items.Clear();

            dbCommand = new SQLiteCommand(sql, dbConnect);
            SQLiteDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read()) {
                bool active = (bool)reader["active"];

                listView_Schedule.Items.Add(new ListViewItem(new string[] {
                    ((int)reader["sethour"]).ToString("00") + " : " +  ((int)reader["setminute"]).ToString("00"),
                    reader["text"].ToString(),
                    active ? "Y" : "N"
                }));
            }

            reader.Close();
            dbConnect.Close();

            button_modifySch.Enabled = false;
            button_deleteSch.Enabled = false;
        }


        // select box data refresh. 
        public void selectBoxDataRefresh(ListBox selectBox, string[] dateStr) {
            int dayItem = (int)selectBox.Items[0];
            selectBox.Items.Clear();
            selectBox.Items.Insert(0, dayItem);

            string sql = "select text from calendarlist " +
                         $"where year = {dateStr[0]} AND month = {dateStr[1]} AND day = {dateStr[2]} " +
                         "order by sethour, setminute ASC";

            dbConnect.Open();
            SQLiteCommand command = new SQLiteCommand(sql, dbConnect);
            SQLiteDataReader reader = command.ExecuteReader();

            int moreCount = 0;
            for (int count = 1; reader.Read(); count = count + 1)
            {
                if (count >= 4) moreCount = moreCount + 1;
                else selectBox.Items.Insert(count, reader["text"].ToString());
            }

            if (moreCount > 0) selectBox.Items.Insert(4, "(...more " + moreCount + ")");
            reader.Close();
            dbConnect.Close();
        }


        // delete select database.                              
        private void deleteDBdata()
        {
            string[] splitstr = new string[2];
            int[] datetemp = new int[2];
            splitstr = listView_Schedule.SelectedItems[0].Text.Split(':');
            datetemp[0] = int.Parse(splitstr[0]);
            datetemp[1] = int.Parse(splitstr[1]);

            // MessageBox.Show(listView_Schedule.SelectedItems[0].SubItems[1].ToString());

            string sql = $"delete from calendarlist where year = {selectYear} AND month = {selectMonth} AND day = {selectDay} AND sethour = {datetemp[0]} AND setminute = {datetemp[1]}";
            dbConnect.Open();

            dbCommand = new SQLiteCommand(sql, dbConnect);
            dbCommand.ExecuteNonQuery();
            dbConnect.Close();

            //changeCalendar();
            // gbox[gbox_index].Items.RemoveAt(calendar_index + 1);
            string[] tempStr = new string[3];
            tempStr[0] = selectYear.ToString();
            tempStr[1] = selectMonth.ToString();
            tempStr[2] = selectDay.ToString();
            selectBoxDataRefresh(gbox[gbox_index], tempStr);


            calendarListRefresh();
        }





        // calendar widget Event.                               
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            int cval = selectDay;
            int temp = selectDay;
            selectDay = int.Parse(e.End.ToString("dd"));

            cval = selectDay - cval;
            if (cval == 0) return;

            if (int.Parse(e.Start.Year.ToString()) == selectYear &&
                int.Parse(e.Start.Month.ToString()) == selectMonth) {
                int[] sat_index = { 13, 20, 27, 34, 41 };
                int[] sun_index = { 7, 14, 21, 28, 35, 42 };
                bool correct_sat_sun_index = false;

                // (blue) 192 192 255 
                foreach (int index in sat_index)
                    if (gbox_index == index)
                    {
                        gbox[gbox_index].BackColor = System.Drawing.Color.FromArgb(192, 192, 255);
                        correct_sat_sun_index = true;
                        break;
                    };

                // (red) 255 192 192 
                if (!correct_sat_sun_index)
                    foreach (int index in sun_index)
                        if (gbox_index == index)
                        {
                            gbox[gbox_index].BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
                            correct_sat_sun_index = true;
                            break;
                        };

                // (white) window. 
                if (!correct_sat_sun_index)
                    gbox[gbox_index].BackColor = System.Drawing.SystemColors.Window;


                // current date color ... skycolor (192 255 255)
                if (DateTime.Now.ToString("yyyy-MM-dd") == (selectYear.ToString() + "-" + selectMonth.ToString("00") + "-" + temp.ToString("00")))
                    gbox[gbox_index].BackColor = System.Drawing.Color.FromArgb(192, 255, 255);


                // selected 
                gbox_index = gbox_index + cval;
                gbox[gbox_index].BackColor = System.Drawing.Color.FromArgb(255, 255, 192);

                button_modifySch.Enabled = false;
                button_deleteSch.Enabled = false;
            }


            else {
                selectYear = int.Parse(e.End.ToString("yyyy"));
                selectMonth = int.Parse(e.End.ToString("MM"));
                changeCalendar();
            }

            label_DateTemp.Text = e.End.ToString("yyyy") + "." + int.Parse(e.End.ToString("MM")).ToString() + "." + int.Parse(e.End.ToString("dd")).ToString();
            label_YearMonth.Text = e.End.ToString("yyyy") + "." + (int.Parse(e.End.ToString("MM")).ToString("00"));
            calendar_index = -1;        // index reset. 
        }


        // click location check Event. (main calender click)    
        private void panel_MonthList_MouseDown(object sender, MouseEventArgs e)
        {
            for (int count = 0; count < gbox.Length; count++)
            {
                if (gbox[count].Location.X <= e.X &&
                    gbox[count].Location.Y <= e.Y &&
                    gbox[count].Location.X + gbox[count].Size.Width > e.X &&
                    gbox[count].Location.Y + gbox[count].Size.Height > e.Y &&
                    gbox[count].TabStop == true)
                {

                    DateTime temp = new DateTime();
                    monthCalendar1.SetDate(temp.AddYears(selectYear - 1).AddMonths(selectMonth - 1).AddDays(int.Parse(gbox[count].Items[0].ToString()) - 1));
                    break;
                }
            }
        }


        // datetime label text changed Event.                   
        private void label_DateTemp_TextChanged(object sender, EventArgs e) { calendarListRefresh(); }


        // schedule click Event.                                
        private void listView_Schedule_Click(object sender, EventArgs e)
        {
            foreach (int getIndex in listView_Schedule.SelectedIndices) calendar_index = getIndex;
            button_modifySch.Enabled = true;
            button_deleteSch.Enabled = true;
        }

        private void listView_Schedule_DoubleClick(object sender, EventArgs e)
        {
            foreach (int getIndex in listView_Schedule.SelectedIndices)
                MessageBox.Show(getIndex.ToString());
        }


        // "ADD" button click Event.                            
        private void button_addSch_Click(object sender, EventArgs e)
        {
            addForm = new DataAddForm(label_DateTemp, this, false);
            addForm.gboxSetting(gbox[gbox_index]);
            addForm.setDbConnect(dbConnect);
            addForm.Show();
        }


        // "Modify" button click Event.                         
        private void button_modifySch_Click(object sender, EventArgs e)
        {
            string[] datalist = new string[2];
            string text = listView_Schedule.SelectedItems[0].SubItems[1].Text.ToString();
            bool actCheck = listView_Schedule.SelectedItems[0].SubItems[2].Text == "Y" ? true : false;

            datalist = listView_Schedule.SelectedItems[0].SubItems[0].Text.Split(':');

            addForm = new DataAddForm(label_DateTemp, this, true);
            addForm.gboxSetting(gbox[gbox_index]);
            addForm.setDbConnect(dbConnect);
            addForm.setSelectData(datalist, text, actCheck);
            addForm.Show();
        }


        // "Delete" button click Event.                         
        private void button_deleteSch_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete the data?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                deleteDBdata();
                refreshAlarm();
            }
        }


        // program close Event.                                 
        private void Form_Calender_main_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                tManager.setThreadEnable(false);
                if (tManager.getThreadManager().ThreadState != System.Threading.ThreadState.Stopped)
                    tManager.getThreadManager().Join();
            }
            catch (NullReferenceException exc) { }
        }


        // get, set Method. 
        public void refreshAlarm() { tManager.nextAlarmReadyRefresh(); }

    }
}