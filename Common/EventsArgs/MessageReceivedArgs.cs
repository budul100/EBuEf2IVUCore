using System;

namespace Common.EventsArgs
{
    public class MessageReceivedArgs
        : EventArgs
    {
        #region Public Constructors

        public MessageReceivedArgs(string content)
        {
            Content = content;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Content { get; private set; }

        #endregion Public Properties
    }
}