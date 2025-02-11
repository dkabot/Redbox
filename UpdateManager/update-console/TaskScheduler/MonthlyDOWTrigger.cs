using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class MonthlyDOWTrigger : StartableTrigger
    {
        public MonthlyDOWTrigger(
          short hour,
          short minutes,
          DaysOfTheWeek daysOfTheWeek,
          WhichWeek whichWeeks,
          MonthsOfTheYear months)
        {
            this.SetStartTime((ushort)hour, (ushort)minutes);
            this.taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_MONTHLYDOW;
            this.taskTrigger.Data.monthlyDOW.WhichWeek = (ushort)whichWeeks;
            this.taskTrigger.Data.monthlyDOW.DaysOfTheWeek = (ushort)daysOfTheWeek;
            this.taskTrigger.Data.monthlyDOW.Months = (ushort)months;
        }

        public MonthlyDOWTrigger(
          short hour,
          short minutes,
          DaysOfTheWeek daysOfTheWeek,
          WhichWeek whichWeeks)
          : this(hour, minutes, daysOfTheWeek, whichWeeks, MonthsOfTheYear.January | MonthsOfTheYear.February | MonthsOfTheYear.March | MonthsOfTheYear.April | MonthsOfTheYear.May | MonthsOfTheYear.June | MonthsOfTheYear.July | MonthsOfTheYear.August | MonthsOfTheYear.September | MonthsOfTheYear.October | MonthsOfTheYear.November | MonthsOfTheYear.December)
        {
        }

        internal MonthlyDOWTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }

        public short WhichWeeks
        {
            get => (short)this.taskTrigger.Data.monthlyDOW.WhichWeek;
            set
            {
                this.taskTrigger.Data.monthlyDOW.WhichWeek = (ushort)value;
                this.SyncTrigger();
            }
        }

        public DaysOfTheWeek WeekDays
        {
            get => (DaysOfTheWeek)this.taskTrigger.Data.monthlyDOW.DaysOfTheWeek;
            set
            {
                this.taskTrigger.Data.monthlyDOW.DaysOfTheWeek = (ushort)value;
                this.SyncTrigger();
            }
        }

        public MonthsOfTheYear Months
        {
            get => (MonthsOfTheYear)this.taskTrigger.Data.monthlyDOW.Months;
            set
            {
                this.taskTrigger.Data.monthlyDOW.Months = (ushort)value;
                this.SyncTrigger();
            }
        }
    }
}
