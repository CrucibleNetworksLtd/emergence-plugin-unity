using System;

namespace EmergenceSDK.Types
{
    public class EmergencePersona
    {
        public string Id { get; }
        public string Name { get; }
        public string Bio { get; }
        public EmergencePersonaSettings Settings { get; }
        [Obsolete] public EmergenceAvatar Avatar { get; }
        public string AvatarId { get; }
    }

    public class EmergencePersonaSettings
    {
        bool showStatus;


        bool receiveContactRequest;


        bool availableOnSearch;

        public EmergencePersonaSettings(bool availableOnSearch, bool receiveContactRequest, bool showStatus)
        {
            this.availableOnSearch = availableOnSearch;
            this.receiveContactRequest = receiveContactRequest;
            this.showStatus = showStatus;
        }
    }
}