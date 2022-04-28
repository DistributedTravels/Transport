namespace Models.Transport
{
    public class ReserveTransportEvent : EventModel
    {
        // new data to be transfered/saved to DB
        // add new data fields + a contstructor for them

        // no need to add the [JsonProperty] here according to MS Documentation/Source Code
        public int DestId { get; set; } // some transfarable data

        public ReserveTransportEvent(int DestId) : base() // be wary of the base() call
        {
            this.DestId = DestId;
        }
    }
}
