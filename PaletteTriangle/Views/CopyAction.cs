using System.Windows;
using Livet.Behaviors.Messaging;
using Livet.Messaging;

namespace PaletteTriangle.Views
{
    public class CopyAction : InteractionMessageAction<DependencyObject>
    {
        protected override void InvokeAction(InteractionMessage message)
        {
            var msg = message as GenericInteractionMessage<string>;
            if (msg != null)
                Clipboard.SetText(msg.Value);
        }
    }
}
