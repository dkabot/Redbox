using System;
using TaskSchedulerInterop;

namespace TaskScheduler
{
    internal class MonthlyTrigger : StartableTrigger
    {
        public MonthlyTrigger(short hour, short minutes, int[] daysOfMonth, MonthsOfTheYear months)
        {
            this.SetStartTime((ushort)hour, (ushort)minutes);
            this.taskTrigger.Type = TaskTriggerType.TIME_TRIGGER_MONTHLYDATE;
            this.taskTrigger.Data.monthlyDate.Months = (ushort)months;
            this.taskTrigger.Data.monthlyDate.Days = (uint)MonthlyTrigger.IndicesToMask(daysOfMonth);
        }

        public MonthlyTrigger(short hour, short minutes, int[] daysOfMonth)
          : this(hour, minutes, daysOfMonth, MonthsOfTheYear.January | MonthsOfTheYear.February | MonthsOfTheYear.March | MonthsOfTheYear.April | MonthsOfTheYear.May | MonthsOfTheYear.June | MonthsOfTheYear.July | MonthsOfTheYear.August | MonthsOfTheYear.September | MonthsOfTheYear.October | MonthsOfTheYear.November | MonthsOfTheYear.December)
        {
        }

        internal MonthlyTrigger(ITaskTrigger iTrigger)
          : base(iTrigger)
        {
        }

        public MonthsOfTheYear Months
        {
            get => (MonthsOfTheYear)this.taskTrigger.Data.monthlyDate.Months;
            set
            {
                this.taskTrigger.Data.monthlyDOW.Months = (ushort)value;
                this.SyncTrigger();
            }
        }

        private static int[] MaskToIndices(int mask)
        {
            int length = 0;
            for (int index = 0; mask >> index > 0; ++index)
                length += 1 & mask >> index;
            int[] indices = new int[length];
            int num = 0;
            for (int index = 0; mask >> index > 0; ++index)
            {
                if ((1 & mask >> index) == 1)
                    indices[num++] = index + 1;
            }
            return indices;
        }

        private static int IndicesToMask(int[] indices)
        {
            int mask = 0;
            foreach (int index in indices)
            {
                if (index < 1 || index > 31)
                    throw new ArgumentException("Days must be in the range 1..31");
                mask |= 1 << index - 1;
            }
            return mask;
        }

        public int[] Days
        {
            get => MonthlyTrigger.MaskToIndices((int)this.taskTrigger.Data.monthlyDate.Days);
            set
            {
                this.taskTrigger.Data.monthlyDate.Days = (uint)MonthlyTrigger.IndicesToMask(value);
                this.SyncTrigger();
            }
        }
    }
}
