namespace EmergenceSDK.Implementations.Login.Types
{
    /// <summary>
    /// Represents the phase of the <see cref="LoginStep"/>
    /// </summary>
    public enum StepPhase
    {
        /// <summary>
        /// The very beginning of the step, before any logic has been executed. Useful for updating the UI with spinners, loading bars, informative labels, and placeholders of any kind
        /// </summary>
        Start,
        /// <summary>
        /// The very end of the step, after it has succeeded. Useful for updating UI with newly available information or to represent the current state of the login.
        /// <remarks>
        /// This was named "Success" to avoid any ambiguity about the fact that when a step ends, it has succeeded.
        /// There is no counterpart for failure, use <see cref="LoginManager.loginFailedEvent"/>
        /// </remarks>
        /// </summary>
        Success
    }
}
