using System;

namespace XivCommon {
    /// <summary>
    /// A base class for accessing XivCommon functionality.
    /// </summary>
    public class XivCommonBase : IDisposable {
        /// <summary>
        /// Game functions and events
        /// </summary>
        public GameFunctions Functions { get; }

        /// <summary>
        /// <para>
        /// Construct a new XivCommon base.
        /// </para>
        /// <para>
        /// This will automatically enable hooks based on the hooks parameter.
        /// </para>
        /// </summary>
        /// <param name="hooks">Flags indicating which hooks to enable</param>
        public XivCommonBase(Hooks hooks = HooksExt.DefaultHooks) {
            this.Functions = new GameFunctions(hooks);
        }

        /// <inheritdoc />
        public void Dispose() {
            this.Functions.Dispose();
        }
    }
}
