using System.Collections.Generic;

namespace EmergenceSDK
{
    public delegate void SuccessPersonas(List<Persona> personas, Persona currentPersona);
    public delegate void ErrorCallback(string message, long code);
    
    public delegate void PersonaUpdated(Persona persona);
    public delegate void SuccessGetCurrentPersona(Persona currentPersona);
    
    public delegate void SuccessCreatePersona();
    
    public delegate void SuccessEditPersona();
    
    public delegate void SuccessDeletePersona();
    
    public delegate void SuccessSetCurrentPersona();
}