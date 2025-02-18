using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using UnityEngine;
using XnaFlash;
using XnaFlash.Content;
using XnaFlash.Movie;
//using XnaFlash.Movie;
using XnaFlash.Swf.Tags;
using XnaVG;

public class EditText : ICharacter, XnaFlash.Movie.IDrawable
{
    private DefineEditTextTag defineEditTextTag;
    private ISystemServices services;
    private FlashDocument flashDocument;

    public EditText(DefineEditTextTag defineEditTextTag, ISystemServices services, FlashDocument flashDocument)
    {
        this.defineEditTextTag = defineEditTextTag;
        this.services = services;
        this.flashDocument = flashDocument;
    }

    public ushort ID
    {
        get
        {
            return defineEditTextTag.CharacterID;
        }
    }

    public CharacterType Type { get { return CharacterType.EditText; } }

    public Rectangle? Bounds => defineEditTextTag.Bounds;

    public void Dispose()
    {
        
    }

    bool isShow;

    public void Draw(IVGRenderContext<DisplayState> target)
    {
        if(defineEditTextTag!=null)
        {
            GUI.skin.label.fontStyle = UnityEngine.FontStyle.Bold;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(
                target.State.PathToSurface.Matrix.M31 * StartScript.Instance.Pyv,
                target.State.PathToSurface.Matrix.M32 * StartScript.Instance.Pyv,
                Bounds.Value.Width * StartScript.Instance.Pyv,
                Bounds.Value.Height * StartScript.Instance.Pyv),
                defineEditTextTag.InitialText);

            GUI.skin.label.fontStyle = UnityEngine.FontStyle.Normal;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
           if(!isShow)
            {
                Debug.Log("EditText: " + defineEditTextTag.InitialText);
                isShow = true;
            }
        }
    }

    public XnaFlash.Movie.IDrawable MakeInstance(DisplayObject container, RootMovieClip root)
    {
        return this;
    }
    StageObject Parent;
    public void SetParent(StageObject parent)
    {
        Parent = parent;
    }
}
