using System.Collections.Generic;
using EmergenceSDK.Types;
using UnityEngine;
using Avatar = EmergenceSDK.Types.Avatar;

namespace EmergenceSDK.Services
{
    public delegate void SuccessPersonas(List<Persona> personas, Persona currentPersona);
    
    public delegate void ErrorCallback(string message, long code);
    
    public delegate void PersonaUpdated(Persona persona);
    
    public delegate void SuccessGetCurrentPersona(Persona currentPersona);
    
    public delegate void SuccessCreatePersona();
    
    public delegate void SuccessEditPersona();
    
    public delegate void SuccessDeletePersona();
    
    public delegate void SuccessSetCurrentPersona();
    
    public delegate void SuccessAvatars(List<Avatar> avatar);
    
    public delegate void SuccessAvatar(Avatar avatar);
    
    public delegate void SuccessInventoryByOwner(List<InventoryItem> inventoryItems);
    
    public delegate void SuccessWriteDynamicMetadata(string response);
    
    public delegate void IsConnectedSuccess(bool connected);
    
    public delegate void ReinitializeWalletConnectSuccess(bool disconnected);
    
    public delegate void RequestToSignSuccess(string signedMessage);
    
    public delegate void QRCodeSuccess(Texture2D qrCode, string deviceId);
    
    public delegate void HandshakeSuccess(string walletAddress);
    
    public delegate void CreateWalletSuccess();
    
    public delegate void CreateKeyStoreSuccess();
    
    public delegate void LoadAccountSuccess();
    
    public delegate void BalanceSuccess(string balance);
    
    public delegate void AccessTokenSuccess(string accessToken);
    
    public delegate void ValidateAccessTokenSuccess(bool valid);
    
    public delegate void ValidateSignedMessageSuccess(bool valid);
    
    public delegate void DisconnectSuccess();
    
    public delegate void SuccessFinish();
}