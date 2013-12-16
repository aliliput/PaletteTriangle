using System;

namespace PaletteTriangle.Models
{
    public class CreatedScriptToRunEventArgs : EventArgs
    {
        public CreatedScriptToRunEventArgs(string script)
        {
            this.Script = script;
        }

        public string Script { get; private set; }
    }
}
