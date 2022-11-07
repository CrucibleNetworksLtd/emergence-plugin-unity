using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;


public class EmergencePersona
{
    public string Id { get; }
    public string Name { get; }
    public string Bio { get; }
    public EmergencePersonaSettings Settings { get; }
    [Obsolete]
    public EmergenceAvatar Avatar { get; }
    public string AvatarId { get; }

    public EmergencePersona(string _json)
    {
        var serializedPersona = JsonConvert.DeserializeAnonymousType(_json, new { id = "", name = "", bio = "", settings = "", avatar = "", avatarid = "" });
        Id = serializedPersona.id;
        Name = serializedPersona.name;
        Bio = serializedPersona.bio;
        Settings = JsonConvert.DeserializeObject<EmergencePersonaSettings>(serializedPersona.settings);
        Avatar = JsonConvert.DeserializeObject<EmergenceAvatar>(serializedPersona.avatar);
        AvatarId = serializedPersona.avatarid;
    }
    public EmergencePersona(string _id, string _name, string _bio, EmergencePersonaSettings _settings, EmergenceAvatar _avatar, string _avatarId)
    {
        Id = _id;
        Name = _name;
        Bio = _bio;
        Settings = _settings;
        Avatar = _avatar;
        AvatarId = _avatarId;
    }

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