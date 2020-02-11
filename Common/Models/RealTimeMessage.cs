﻿using Newtonsoft.Json;
using System;

namespace Common.Models
{
    public enum SignalTyp
    {
        ASig,
        BkSig,
        ESig,
    }

    public class RealTimeMessage
    {
        #region Public Properties

        [JsonProperty("betriebsstelle")]
        public string Betriebsstelle { get; set; }

        [JsonProperty("decoder")]
        public string Decoder { get; set; }

        [JsonProperty("end_gleis")]
        public string EndGleis { get; set; }

        [JsonProperty("signaltyp")]
        public SignalTyp SignalTyp { get; set; }

        [JsonProperty("simulationszeit")]
        public DateTime SimulationsZeit { get; set; }

        [JsonProperty("start_gleis")]
        public string StartGleis { get; set; }

        [JsonProperty("ziel_gleis")]
        public string ZielGleis { get; set; }

        [JsonProperty("zugnummer")]
        public string Zugnummer { get; set; }

        #endregion Public Properties

        #region Public Methods

        public override string ToString()
        {
            return $@"Zug: {Zugnummer} | Betriebsstelle: {Betriebsstelle} | Signaltyp: {SignalTyp.ToString()} | " +
                @"Startgleis: {StartGleis} | Zielgleis: {ZielGleis} |  Simulationszeit: {SimulationsZeit:hh\:mm}";
        }

        #endregion Public Methods
    }
}