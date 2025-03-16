using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Reservation.DataFile
{
    public class ReservationDataFile_V001 : IReservationDataFile
    {
        private List<ReservationDataInfo_V001> _dataInfos = new List<ReservationDataInfo_V001>();

        public string HashedCardId { get; set; }

        [JsonIgnore] public string FileName => string.Format("{0}.dat", (object)ID);

        public Guid ID { get; set; }

        public List<ReservationDataInfo_V001> ReservationDataInfos
        {
            get => _dataInfos;
            set => _dataInfos = value;
        }
    }
}