using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XnaFlash.Actions;
using XnaFlash.Actions.Functions;
using XnaFlash.Actions.Objects;
using XnaFlash.Content;
using XnaFlash.Swf.Structures;
using XnaFlash.Swf.Tags;
using XnaVG;

namespace XnaFlash.Movie
{
    public abstract class StageObject : ActionObject, IDrawable, IInstanceable
    {        
        protected DisplayObject _container;
        protected DisplayList _displayList = new DisplayList();
        protected string _name = null;
        protected ActionBlock[] _eventActions = new ActionBlock[_eventNames.Count];
        protected ActionFunc[] _eventFunctions = new ActionFunc[_eventNames.Count];
        protected ActionFunc[] _apiFunctions = new ActionFunc[_apiNames.Count];
        protected ActionBlock _keyPress = null;
        protected KeyCode? _keyPressKeyCode;

        public RootMovieClip Root { get; protected set; }
        public StageObject Parent { get; private set; }        
        public string Path { get { return GetRootedPath(false); } }

        internal StageObject(RootMovieClip root, DisplayObject container)
        {
            _container = container;

            Root = root;            
            Parent = null;
            Visible = true;
            Context = new ActionContext
            {
                Constants = null,
                DefaultTarget = this,
                Registers = new ActionVar[4],
                RootClip = Root,
                Scope = new LinkedList<ActionObject>(),
                Stack = new Stack<ActionVar>(),
                This = this
            };
            if (Root != null)
            {
                Context.Scope.AddFirst(Root.GlobalScope);
                this["_root"] = root;
            }
            Context.Scope.AddLast(this);
        }

        public void SetParent(StageObject parent)
        {
            Parent = parent;
            this["_parent"] = new ActionVar(parent);
        }

        public abstract bool OnMouseMove();
        public virtual void OnNextFrame()
        {            
            _displayList.OnNextFrame();
            RunEvent(Event.onEnterFrame);
        }
        public bool IsEquals(LinkedList<DisplayObject> displayList)
        {
            return displayList .Equals ( _displayList.DisplayListt);
        }
        public virtual void Load()
        {
            RunEvent(Event.onLoad);
        }
        public virtual void Unload()
        {
            RunEvent(Event.onUnload);
        }             
        public virtual string GetRootedPath(bool slashes)
        {
            if (Parent == null || Name == null) return null;
            var p = Parent.GetRootedPath(slashes);
            return p != null ? (p + (slashes ? '/' : '.') + Name) : null;
        }
        public virtual void CloneTo(ushort depth, string name)
        {
            if (Parent == null) return;
            // TODO
        } 
        public virtual void Remove()
        {
            if (Parent == null) return;
            Parent._displayList.Remove(_container.Depth);
        }
        public virtual void SetName(string name)
        {
            if (Parent == null) return;
            if (!string.IsNullOrEmpty(_name))
                Parent[_name] = new ActionVar();

            if (!string.IsNullOrEmpty(name))
                Parent[name] = this;
            _name = name;
        }

        public StageObject GetInstanceByPath(string path,bool isGetObj=true)
        {
            if (string.IsNullOrEmpty(path)) return this;

            StageObject o = this;
            if (path[0] != '_')
            {
                if (path[0] == '/')
                {
                    o = Root;
                    path = path.Substring(1);
                }

                string[] tokens = path.Split('/');
                foreach (var t in tokens)
                {
                    if (t == "..")
                        o = o.Parent;
                    else if (t == ".")
                    { 
                    }
                    else
                    {
                        if(t.Contains(':'))
                        {
                            o = o.Parent;
                        }
                        else
                        o = o[t].Object as StageObject;
                    }

                    if (o == null)
                        return this;
                }
            }
            else
            {
                if(!isGetObj)
                {
                    return this;
                }
                else
                {
                    string[] tokens = path.Split('.');
                    foreach (var t in tokens)
                    {
                        o = o[t].Object as StageObject;
                        if (o == null)
                            return this;
                    }
                }
               
            }

            return o;
        }
        public void SetClipActions(ClipActions actions)
        {
            foreach (var action in actions.Records)
            {
                foreach (var e in action.Events)
                    _eventActions[(int)e] = action.Actions;

                if ((action.EventFlags & ClipEventFlags.KeyPress) != 0)
                {
                    _keyPressKeyCode = action.KeyCode;
                    _keyPress = action.Actions;
                }
            }
        }

        void IDrawable.Draw(IVGRenderContext<DisplayState> target)
        {
            if (Visible)
                _displayList.Draw(target);
        }
        protected override string AsString()
        {
            return "[" + Name + "]";
        }

        #region Native properties & Events

        private static Dictionary<string, Properties> _propNames = new Dictionary<string, Properties>(1 + (int)Properties._mousey);
        private static Dictionary<string, Event> _eventNames = new Dictionary<string, Event>(1 + (int)Event.onUnload);
        private static Dictionary<string, Api> _apiNames = new Dictionary<string, Api>(1 + (int)Api.onUnload);

        public virtual bool Visible { get; set; }
        public virtual float X
        {
            get { return _container.Matrix.M31; }
            set { _container.Matrix.M31 = value; }
        }
        public virtual float Y
        {
            get { return _container.Matrix.M32; }
            set { _container.Matrix.M32 = value; }
        }
        public virtual float XScale
        {
            get { return _container.Matrix.ScaleX * 100f; }
            set { _container.Matrix.ScaleX = value / 100f; }
        }
        public virtual float YScale
        {
            get { return _container.Matrix.ScaleX * 100f; }
            set { _container.Matrix.ScaleX = value / 100f; }
        }
        public virtual float Alpha
        {
            get { return _container.Alpha; }
            set { _container.Alpha = value; }
        }
        public virtual float Width
        {
            get
            {
                return 1f;  //Bounds.Width* XScale; Compute from displayList
            }
            set
            {
                //XScale = value / Bounds.Width;
            }
        }
        public virtual float Height
        {
            get
            {
                return 1f; // Bounds.Height * YScale; compute
            }
            set
            {
                //YScale = value / Bounds.Height;
            }
        }
        public virtual float Rotation
        {
            get { return MathHelper.ToDegrees(_container.Matrix.Rotation); }
            set { _container.Matrix.Rotation = MathHelper.ToRadians(value); }
        }
        public virtual string Target
        {
            get { return GetRootedPath(true); }
        }
        public virtual string Name
        {
            get { return _name; }
        }
        public virtual bool FocusRect
        {
            get { return false; }
            set { }
        }
        public virtual float SoundBufTime
        {
            get { return 0f; }
            set { }
        }
        public virtual int HighQuality
        {
            get
            {
                return Root.HighQuality;
            }
            set
            {
                Root.HighQuality = value;
            }
        }
        public virtual string Quality
        {
            get { return Root.Quality; }
            set { Root.Quality = value; }
        }
        public virtual float MouseX
        {
            get { return Root.MouseX - X; }
        }
        public virtual float MouseY
        {
            get { return Root.MouseY - Y; }
        }
        public virtual string Url
        {
            get { return Root.Url; }
        }

        public virtual ActionVar this[Properties prop]
        {
            get
            {
                switch (prop)
                {
                    case Properties._x: return X;
                    case Properties._y: return Y;
                    case Properties._xscale: return XScale;
                    case Properties._yscale: return YScale;
                    case Properties._alpha: return Alpha;
                    case Properties._visible: return Visible;
                    case Properties._width: return Width;
                    case Properties._height: return Height;
                    case Properties._rotation: return Rotation;
                    case Properties._target: return Target;
                    case Properties._name: return Name;
                    case Properties._url: return Url;
                    case Properties._highquality: return HighQuality;
                    case Properties._focusrect: return FocusRect;
                    case Properties._soundbuftime: return SoundBufTime;
                    case Properties._quality: return Quality;
                    case Properties._mousex: return MouseX;
                    case Properties._mousey: return MouseY;
                    case Properties.byteloaded: return BytesLoaded;
                    case Properties.bytetotal: return BytesTotal;
                    case Properties.keycode: return (int)UnityEngine.KeyCode.None;

                    default:
                        return this[(int)prop];
                }
            }
            set
            {
                switch (prop)
                {
                    case Properties._x: X = (float)value; break;
                    case Properties._y: Y = (float)value; break;
                    case Properties._xscale: XScale = (float)value; break;
                    case Properties._yscale: YScale = (float)value; break;
                    case Properties._alpha: Alpha = (float)value; break;
                    case Properties._visible: Visible = (bool)value; break;
                    case Properties._width: Width = (float)value; break;
                    case Properties._height: Height = (float)value; break;
                    case Properties._rotation: Rotation = (float)value; break;
                    case Properties._name: SetName(value.String); break;
                    case Properties._highquality: HighQuality = (int)value; break;
                    case Properties._focusrect: FocusRect = (bool)value; break;
                    case Properties._soundbuftime: SoundBufTime = (float)value; break;
                    case Properties._quality: Quality = value.String; break;
                    default:
                        // Root.Trace("Unhandled property {0}!", prop.ToString());
                        this[(int)prop] = value;
                        break;
                }
            }
        }
        
        public override ActionVar this[string name]
        {
            get
            {
                Properties prop;
                Event e;
                Api i;
                if (name != null)
                {
                    if(_propNames.TryGetValue(name, out prop))
                        return this[prop];

                    if (_eventNames.TryGetValue(name, out e))
                        return _eventFunctions[(int)e];
                    if (_apiNames.TryGetValue(name, out i))
                    {
                        if(_apiFunctions[(int)i]!=null)
                        return _apiFunctions[(int)i];
                        else if(name== "getBytesLoaded")
                        {
                            return BytesLoaded;
                        }
                        else if (name == "getBytesTotal")
                        {
                            return BytesTotal;
                        }
                    }
                }
                
                return base[name];
            }
            set
            {
                Properties prop;
                Event e;
                Api i;

                if (name != null)
                {
                    if (_propNames.TryGetValue(name, out prop))
                    {
                        this[prop] = value;
                        return;
                    }

                    if (_eventNames.TryGetValue(name, out e))
                    {
                        _eventFunctions[(int)e] = value.Func;
                        return;
                    }

                    if (_apiNames.TryGetValue(name, out i))
                    {
                        _apiFunctions[(int)i] = value.Func;
                        UnityEngine.Debug.Log("Api Names:"+name);
                        return;
                    }
                }

                base[name] = value;
            }
        }
        protected void RunEvent(Event eventId)
        {
            if (eventId > Event.onUnload)
            {
                Root.Trace("Invalid event has been called!");
                return;
            }

            int ev = (int)eventId;
            if (_eventActions[ev] != null)
                _eventActions[ev].RunSafe(Context);

            if (_eventFunctions[ev] != null)
                _eventFunctions[ev].Invoke(Context);
        }

        #endregion

        #region Static Constructor

        static StageObject()
        {
            for (int i = 0; i <= (int)Event.onUnload; i++)
                _eventNames.Add(((Event)i).ToString(), (Event)i);
            int ApiLength = Enum.GetValues(typeof(Api)).Length;
            for (int i = 0; i <= ApiLength; i++)
                _apiNames.Add(((Api)i).ToString(), (Api)i);
            int length= Enum.GetValues(typeof(Properties)).Length;
            for (int i = 0; i < length; i++)
                _propNames.Add(((Properties)i).ToString(), (Properties)i);
        }

        #endregion
    }
}
