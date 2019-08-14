﻿using System;
using System.Windows.Forms;

namespace CalenderWinForm
{
    public partial class Form_Calender_main : Form {

        // variable. 
        private ThreadManager tManager;
        private ListBox[] gbox;
        private int selectYear;
        private int selectMonth;
        private int selectDay;
        private bool refreshCheck;


        // main method. 
        public Form_Calender_main() {
            InitializeComponent();
            gbox = new ListBox[42];
            tManager = new ThreadManager(label_Time);

            // Panel setting. 
            for(int count = 0; count < gbox.Length; count++) gbox[count] = new ListBox();


            // Current date setting. 
            selectDay = int.Parse(DateTime.Now.ToString("dd"));
            numericUpDown_Year.Value = selectYear = int.Parse(DateTime.Now.ToString("yyyy"));
            numericUpDown_Month.Value = selectMonth = int.Parse(DateTime.Now.ToString("MM"));

            for (int row = 1, count = 0; row <= 6; row++)
                for (int col = 0; col < 7; col++) { panel_MonthList.Controls.Add(gbox[count], col, row); count++; }
        }



        // calender diary set Method. 
        private void settingCalender() {
            
            int maxDays = int.Parse(DateTime.DaysInMonth(selectYear, selectMonth).ToString());
            int blankCount, tempCt;
            DateTime dOfMonth = new DateTime();

            dOfMonth = dOfMonth.AddYears(selectYear - 1).AddMonths(selectMonth - 1);
            refreshCheck = true;

            switch (dOfMonth.DayOfWeek.ToString()){
                case "Sunday":      blankCount = 7; break;
                case "Monday":      blankCount = 1; break;
                case "Tuesday":     blankCount = 2; break;
                case "Wednesday":   blankCount = 3; break;
                case "Thursday":    blankCount = 4; break;
                case "Friday":      blankCount = 5; break;
                default:            blankCount = 6; break;
            }

            tempCt = blankCount - 1;

            for (int row = 1, boxCount = 0, dayCount = 1; row <= 6; row = row + 1 )
                for(int col = 0; col < 7; col = col + 1){

                    if (blankCount > 0 || dayCount > maxDays) {
                        gbox[boxCount].BackColor = System.Drawing.SystemColors.InactiveCaptionText;
                        blankCount = blankCount - 1;
                    }

                    else {

                        // Calender Panel color setting.
                        if (DateTime.Now.ToString("yyyy-MM") == (selectYear.ToString() + "-" + selectMonth.ToString("00")) && dayCount == int.Parse(DateTime.Now.ToString("dd")))
                            gbox[dayCount + tempCt].BackColor = System.Drawing.Color.FromArgb(192, 255, 255);

                        else if (dOfMonth.DayOfWeek.ToString() == "Sunday")
                            gbox[boxCount].BackColor = System.Drawing.Color.FromArgb(255, 192, 192);

                        else if (dOfMonth.DayOfWeek.ToString() == "Saturday")
                            gbox[boxCount].BackColor = System.Drawing.Color.FromArgb(192, 192, 255);

                        else gbox[boxCount].BackColor = System.Drawing.SystemColors.Window;

                        gbox[boxCount].Items.Insert(0, dayCount);
                        dayCount = dayCount + 1;
                        //MessageBox.Show(dOfMonth.DayOfWeek.ToString() + " " + dOfMonth.Date.ToString());
                        dOfMonth = dOfMonth.AddDays(1);
                    }

                    gbox[boxCount].Enabled = false;
                    boxCount = boxCount + 1;
                    
                }

           gbox[selectDay + tempCt].BackColor = System.Drawing.Color.FromArgb(255, 255, 192);
        }


        // ***** Event Method.  ***** 
        private void numericUpDown_Year_ValueChanged(object sender, EventArgs e) {
            changeCalender();
            DateTime temp = new DateTime();
            monthCalendar1.SetDate(temp.AddYears(selectYear - 1).AddMonths(selectMonth - 1).AddDays(selectDay - 1));
        }

        private void numericUpDown_Month_ValueChanged(object sender, EventArgs e) {
            changeCalender();
            DateTime temp = new DateTime();
            monthCalendar1.SetDate(temp.AddYears(selectYear - 1).AddMonths(selectMonth - 1).AddDays(selectDay - 1));
        }

        private void changeCalender()
        {
            for (int count = 0; count < gbox.Length; count++) gbox[count].Items.Clear();

            selectYear = int.Parse(numericUpDown_Year.Value.ToString());
            selectMonth = int.Parse(numericUpDown_Month.Value.ToString());
            settingCalender();
        }


        // calender widget Event. 
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e) {

            selectDay = int.Parse(e.End.ToString("dd"));
            numericUpDown_Year.Value = selectYear = int.Parse(e.End.ToString("yyyy"));
            numericUpDown_Month.Value = selectMonth = int.Parse(e.End.ToString("MM"));

            if (!refreshCheck) changeCalender(); refreshCheck = false;

            label_DateTemp.Text = e.End.ToString("yyyy") + "." + int.Parse(e.End.ToString("MM")).ToString() + "." + int.Parse(e.End.ToString("dd")).ToString();
        }


        // program close. 
        private void Form_Calender_main_FormClosed(object sender, FormClosedEventArgs e) {
            try {
                tManager.setThreadEnable(false);
                if (tManager.getThreadManager().ThreadState != System.Threading.ThreadState.Stopped)
                    tManager.getThreadManager().Join();
            }
            catch (NullReferenceException exc) { }
        }



    }
}

// Background Image : 
// https://pixabay.com/ko/photos/%EB%B2%BD%EC%A7%80-%EA%B3%B5%EA%B0%84-%EB%B0%94%ED%83%95-%ED%99%94%EB%A9%B4-%EC%9A%B0%EC%A3%BC-3584226/
