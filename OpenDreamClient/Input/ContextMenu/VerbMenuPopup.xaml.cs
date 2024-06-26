using System.Linq;
using OpenDreamClient.Interface.Controls;
using OpenDreamShared.Dream;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Client.ViewVariables;

namespace OpenDreamClient.Input.ContextMenu;

[GenerateTypedNameReferences]
internal sealed partial class VerbMenuPopup : Popup {
    public delegate void VerbSelectedHandler();

    public VerbSelectedHandler? OnVerbSelected;

    private readonly ClientVerbSystem? _verbSystem;

    private readonly ClientObjectReference _target;

    public VerbMenuPopup(ClientVerbSystem? verbSystem, sbyte seeInvisible, ClientObjectReference target) {
        RobustXamlLoader.Load(this);

        _verbSystem = verbSystem;
        _target = target;

        if (verbSystem != null) {
            var sorted = verbSystem.GetExecutableVerbs(_target).Order(VerbNameComparer.OrdinalInstance);

            foreach (var (verbId, verbSrc, verbInfo) in sorted) {
                if (verbInfo.IsHidden(false, seeInvisible))
                    continue;
                if(!verbInfo.ShowInPopupAttribute)
                    continue;

                AddVerb(verbId, verbSrc, verbInfo);
            }
        }

#if TOOLS
        // If we're compiling with TOOLS and this is an entity, provide the option to use RT's VV on it
        if (_target.Type == ClientObjectReference.RefType.Entity) {
            var viewVariablesButton = AddButton("RT ViewVariables");

            viewVariablesButton.OnPressed += _ => {
                IoCManager.Resolve<IClientViewVariablesManager>().OpenVV(_target.Entity);
            };
        }
#endif
    }

    private void AddVerb(int verbId, ClientObjectReference verbSrc, VerbSystem.VerbInfo verbInfo) {
        var button = AddButton(verbInfo.Name);
        var takesTargetArg = verbInfo.GetTargetType() != null && !verbSrc.Equals(_target);

        button.OnPressed += _ => {
            _verbSystem?.ExecuteVerb(verbSrc, verbId, takesTargetArg ? [_target] : []);
        };
    }

    private Button AddButton(string text) {
        var button = new Button {
            Text = text
        };

        button.OnPressed += _ => {
            Close();
            OnVerbSelected?.Invoke();
        };

        VerbMenu.AddChild(button);
        return button;
    }
}
