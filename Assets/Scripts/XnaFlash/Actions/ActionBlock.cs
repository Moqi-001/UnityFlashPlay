//#define ACTIONS_DISABLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using XnaFlash.Actions.Functions;
using XnaFlash.Actions.Objects;
using XnaFlash.Actions.Records;
using XnaFlash.Movie;

namespace XnaFlash.Actions
{
    using Scope = LinkedListNode<ActionObject>;
    public class ActionBlock
    {
#if !ACTIONS_DISABLED
        private static ConstantPool pool;
#endif

        private ActionCode[] _actions;
        private Dictionary<int, ActionRecord> _payloads;

        internal ActionBlock(params ActionRecord[] actions)
        {
            var branches = new Dictionary<int, List<int>>();
            _payloads = new Dictionary<int, ActionRecord>();
            _actions = new ActionCode[actions.Length];

            for (int i = 0; i < actions.Length; i++)
            {
                _actions[i] = actions[i].Action;
                if (typeof(ActionRecord) != actions[i].GetType())
                    _payloads.Add(i, actions[i]);
                if (actions[i] is BranchAction)
                {
                    var branch = actions[i] as BranchAction;
                    int address = branch.BranchAddress;
                    if (branches.ContainsKey(address))
                        branches[address].Add(i);
                    else
                        branches.Add(address, new List<int> { i });
                }
            }

            for (int i = 0; i < actions.Length; i++)
            {
                if (!branches.ContainsKey(actions[i].Address))
                    continue;

                foreach (var branch in branches[actions[i].Address])
                    _payloads[branch] = new IndexBranchAction(i);
            }
        }
        public static string Parent = "_parent";
        public static string Root = "_root";
        public static string Global = "_global";

        

        private ActionVar GetVariable(StageObject target, ActionContext context, string name, out Scope scope)
        {
            scope = context.Scope.Last;
            ActionVar v;
            StageObject obj = target;
            int colon = name.LastIndexOf(':');
            string var = name;
            string path = name;

            if (colon != -1)
            {
                path = name.Substring(0, colon);
                obj = target.GetInstanceByPath(path, false);
                colon++;
                var = name.Substring(colon, name.Length- colon);
            }
            else
            {
                if(name.Contains ('/'))
                {
                   return target.GetInstanceByPath(path, false);
                }
            }

            v = obj[var];
            return v;
        }

        private void SetVariable(StageObject target,ActionContext context, string name, ActionVar value)
        {
            StageObject obj = target;
            int colon = name.LastIndexOf(':');
            string var = name;
            string path = name;

            if (colon != -1)
            {
                path = name.Substring(0, colon);
                obj = target.GetInstanceByPath(path, false);
                colon++;
                var = name.Substring(colon, name.Length - colon);
            }
            else
            {
                if (name.Contains('/'))
                {
                    obj= target.GetInstanceByPath(path, true);
                }
            }

            obj[var]= value;
        }

        public ActionVar GetProperty(StageObject target, ActionContext context, string targetName, Properties property)
        {
            if(targetName.Length<=0)
            {
               return target[property];
            }
            if(targetName[0]=='/')
            {
                return context.RootClip.GetInstanceByPath(targetName)[property];
            }
            else
            {
                return target.GetInstanceByPath(targetName)[property];
            }
        }

        public void SetProperty(StageObject target, ActionContext context, string path, Properties property, ActionVar value)
        {
            target.GetInstanceByPath(path)[property] = value;
        }

        public ActionVar RunSafe(ActionContext context)
        {
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
            {
                return RunUnsafe(context);
            }
            else
            {
                try
                {
                    return RunUnsafe(context);
                }
                catch (Exception e)
                {
                    context.RootClip.Trace("Action exception: {0}", e.Message);
                    return new ActionVar();
                }
            }
        }
        public int Loaded = 0;
        public int Total = 1;
        Stack<ActionVar> stack;
        static int index;
        public ActionVar RunUnsafe(ActionContext context)
        {            
#if ACTIONS_DISABLED
            return new ActionVar();
#else
            stack = context.Stack;
            StageObject sprite, target;
            index++;
            context.Constants = pool;
            sprite = (context.This as MovieClip);
            if (context.This is MovieClip)
                target = sprite;
            else if (context.This is Button)
            {
                target = (context.This as Button).Parent as MovieClip ?? context.DefaultTarget;
                sprite = target;
            }
            else 
                target = context.DefaultTarget;
                
            for (int i = 0, count = _actions.Length; i < count; i++)
            {
                
                switch (_actions[i])
                {
                    #region Return

                    case ActionCode.End:
                        return new ActionVar();

                    case ActionCode.Return:
                        return stack.Pop();

                    #endregion

                    #region SWF 3

                    case ActionCode.GoToFrame:
                        if (target is MovieClip)
                        {
                            if(context.i<1000)
                            {
                                MovieClip movieClip = target as MovieClip;
                                int value = (_payloads[i] as FrameAction).Frame;
                                //if(value==166 || value == 306 || value == 102)
                                //    movieClip.GoTo(value);
                                //else
                                    movieClip.GoTo(value + 1);
                                //movieClip.Play();
                            }
                            else
                            {
                                if(UnityEngine.Application.platform==UnityEngine.RuntimePlatform.WindowsEditor)
                                          UnityEngine.Debug.LogWarning("Skip Frame!");
                                context.i = 0;
                            }
                        }
                        break;
                    case ActionCode.NextFrame:
                        if (target is MovieClip)
                            (target as MovieClip).NextFrame();
                        break;
                    case ActionCode.PrevFrame:
                        if (target is MovieClip)
                            (target as MovieClip).PrevFrame();
                        break;
                    case ActionCode.Play:
                        if (target is MovieClip)
                            (target as MovieClip).Play();
                        break;
                    case ActionCode.Stop:
                        if (target is MovieClip)
                            (target as MovieClip).Stop();
                        break;
                    case ActionCode.ToggleQuality:
                        context.RootClip.ToggleQuality();
                        break;
                    case ActionCode.StopSounds:
                        context.RootClip.StopSounds();
                        break;
                    case ActionCode.SetTarget:
                        SetTargetAction targetAction = _payloads[i] as SetTargetAction;
                        //if (sprite == null)
                        //    target = target.GetInstanceByPath(targetAction.Target);
                        //else
                        //    target = sprite.GetInstanceByPath(targetAction.Target);

                        if (targetAction.Target.Length <=0)
                            target = sprite;
                        else
                            target = target.GetInstanceByPath(targetAction.Target);
                        break;
                    case ActionCode.GoToLabel:
                        int Frame = (target as MovieClip).GetFrameByLabel((_payloads[i] as GoToLabelAction).Label);
                        (target as MovieClip).GoTo(Frame+1);
                        break;

                    #endregion

                    #region SWF 4

                    case ActionCode.Push:
                        var values = (_payloads[i] as PushAction).Values;
                        for (int j = 0; j < values.Length; j++)
                        {
                            if (values[j] is ActionVar.IndexActionVar)
                                stack.Push(context.Constants[values[j]]);
                            else
                                stack.Push(values[j]);
                        }
                        break;
                    case ActionCode.Pop:
                        stack.Pop();
                        break;                    
                    case ActionCode.Add:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B + A);
                        }
                        break;
                    case ActionCode.Subtract:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B - A);
                        }
                        break;
                    case ActionCode.Multiply:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B * A);
                        }
                        break;
                    case ActionCode.Divide:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B / A);
                        }
                        break;
                    case ActionCode.Equals:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B == A);
                        }
                        break;
                    case ActionCode.Less:
                        {
                            var A = stack.Pop().Double;
                            var B = stack.Pop().Double;
                            stack.Push(B < A);
                        }
                        break;
                    case ActionCode.And:
                        {
                            var A = stack.Pop().Boolean;
                            var B = stack.Pop().Boolean;
                            stack.Push(B && A);
                        }
                        break;
                    case ActionCode.Or:
                        {
                            var A = stack.Pop().Boolean;
                            var B = stack.Pop().Boolean;
                            stack.Push(B || A);
                        }
                        break;
                    case ActionCode.Not:
                        stack.Push(!stack.Pop().Boolean);
                        break;
                    case ActionCode.StringEquals:
                        {
                            var A = stack.Pop().String;
                            var B = stack.Pop().String;
                            stack.Push(B == A);
                        }
                        break;
                    case ActionCode.StringLength:
                    case ActionCode.MBStringLength:
                        stack.Push(stack.Pop().String.Length);
                        break;
                    case ActionCode.StringAdd:
                        {
                            var A = stack.Pop().String;
                            var B = stack.Pop().String;
                            stack.Push(B + A);
                        }
                        break;
                    case ActionCode.StringExtract:
                    case ActionCode.MBStringExtract:
                        {
                            var A = stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(stack.Pop().String.Substring((int)B, (int)A));
                        }
                        break;
                    case ActionCode.StringLess:
                        {
                            var A = stack.Pop().String;
                            var B = stack.Pop().String;
                            stack.Push(B.CompareTo(A) < 0);
                        }
                        break;
                    case ActionCode.ToInteger:
                        stack.Push(stack.Pop().Integer);
                        break;
                    case ActionCode.ToAscii:
                    case ActionCode.MBToAscii:
                        stack.Push((long)stack.Pop().FirstChar);
                        break;
                    case ActionCode.ToChar:
                    case ActionCode.MBToChar:
                        stack.Push(new string((char)stack.Pop().Integer, 1));
                        break;
                    case ActionCode.Jump:
                        IndexBranchAction index = _payloads[i] as IndexBranchAction;
                        if(index==null)
                        {
                            BranchAction record = _payloads[i] as BranchAction;
                            i = record.BranchAddress - 1;
                        }
                        else
                        {
                            i = index.BranchAddress - 1;
                        }
                        break;
                    case ActionCode.If:
                        if (stack.Pop().Boolean)
                            i = (_payloads[i] as IndexBranchAction).BranchAddress - 1;
                        break;
                    case ActionCode.Call:
                        {
                            // FIXME: Dont know if this context approach is correct!
                            var frame = stack.Pop();
                            if (!(sprite is MovieClip)) 
                                break;

                            var mcSprite = sprite as MovieClip;
                            var action = mcSprite.GetFrameAction(frame.IsString ? mcSprite.GetFrameByLabel(frame.String) : (ushort)frame.Integer);
                            if (action != null && action.Length > 0)
                                foreach(var a in action)
                                    a.RunUnsafe(context);
                        }
                        break;
                    case ActionCode.GetVariable:
                        {
                            Scope scope;
                            stack.Push(GetVariable(target, context, stack.Pop().String, out scope));
                        }
                        break;
                    case ActionCode.SetVariable:
                        {
                            var value = stack.Pop();
                            var name = stack.Pop().String;
                            SetVariable(target, context, name, value);
                        }
                        break;
                    case ActionCode.GoToFrame2:
                        {
                            var frame = stack.Pop();
                            var payload = _payloads[i] as GoToFrame2Action;

                            if (!(target is MovieClip))
                                break;

                            var mcSprite = target as MovieClip;
                            if (payload.Play)
                                mcSprite.Play();
                            else
                                mcSprite.Stop();
                            int gotoFrame = payload.SceneBias;
                            if (frame.IsInteger)
                                gotoFrame += (int)frame.Integer;
                            else
                                gotoFrame += mcSprite.GetFrameByLabel(frame.String);
                            mcSprite.GoTo(gotoFrame+1);
                        }
                        break;
                    case ActionCode.SetTarget2:
                        target = sprite.GetInstanceByPath(stack.Pop().String);
                        break;
                    case ActionCode.GetProperty:
                        {
                            var property = (Properties)stack.Pop().Integer;
                            var targetName = stack.Pop().String;
                            
                            stack.Push(GetProperty(target , context, targetName, property));
                        }
                        break;
                    case ActionCode.SetProperty:
                        {
                            var value = stack.Pop();
                            var property = (Properties)stack.Pop().Integer;
                            var path = stack.Pop().String;
                            SetProperty(target, context, path, property,value);
                        }
                        break;
                    case ActionCode.CloneSprite:
                        {
                            var depth = (ushort)stack.Pop().Integer;
                            var name = stack.Pop().String;
                            var source = sprite.GetInstanceByPath(stack.Pop().String);
                            if (source != null)
                                source.CloneTo(depth, name);
                        }
                        break;
                    case ActionCode.RemoveSprite:
                        {
                            var source = sprite.GetInstanceByPath(stack.Pop().String);
                            if (source != null)
                                source.Remove();
                        }
                        break;
                    case ActionCode.StartDrag:
                        {
                            //MovieClip draggable = null;
                            var draggable = sprite.GetInstanceByPath(stack.Pop().String);
                            var lockCenter = stack.Pop().Boolean;
                            Rectangle? constraint = null;
                            if (stack.Pop().Boolean)
                            {
                                var y2 = (int)stack.Pop().Integer;
                                var x2 = (int)stack.Pop().Integer;
                                var y1 = (int)stack.Pop().Integer;
                                var x1 = (int)stack.Pop().Integer;
                                constraint = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                            }
                            if (draggable != null && draggable is MovieClip)
                                (draggable as MovieClip).StartDrag(lockCenter, constraint);
                        }
                        break;
                    case ActionCode.EndDrag:
                        context.RootClip.EndDrag();
                        break;
                    case ActionCode.Trace:
                        context.RootClip.Trace(stack.Pop().String);
                        break;
                    case ActionCode.GetTime:
                        stack.Push(context.RootClip.Runtime);
                        break;
                    case ActionCode.RandomNumber:
                        stack.Push(context.RootClip.GetRandom((int)stack.Pop().Integer));                        
                        break;

                    #endregion

                    #region SWF 5

                    case ActionCode.CallFunction:
                        {
                            Scope scope;
                            ActionVar function = GetVariable(target, context, stack.Pop().String, out scope);
                            var parms = new ActionVar[stack.Pop().Integer];
                            for (int p = 0; p < parms.Length; p++)
                                parms[p] = stack.Pop();

                            stack.Push(function.IsFunc ? function.Func.Invoke(context, parms) : new ActionVar());
                        }
                        break;
                    case ActionCode.CallMethod:
                        {
                            ActionFunc func = null;
                            var funcName = stack.Pop().NullableString;

                            var obj = stack.Pop().WrapperObject;
                            if (obj != null)
                                func = string.IsNullOrEmpty(funcName) ? obj.Invokable : obj[funcName].Func;

                            var parms = new ActionVar[stack.Pop().Integer];
                            for (int p = 0; p < parms.Length; p++)
                                parms[p] = stack.Pop();

                            stack.Push(func != null ? func.Invoke(/*obj.Context*/ context, parms) : new ActionVar());

                            if (FlashDocument.Version >=5)
                            {
                                if (funcName == "gotoAndPlay")
                                {
                                    //stack.Pop();
                                    MovieClip movieClip = obj as MovieClip;
                                    if (movieClip == null)
                                    {
                                        movieClip = context.RootClip;
                                    }

                                    if (parms.Length == 1)
                                    {
                                        var v = parms[0];
                                        if (v.IsInteger)
                                        {
                                            movieClip.GoTo((ushort)(v.Integer-1));
                                        }
                                        else
                                        {
                                            movieClip.GoTo(v.String);
                                        }
                                    }
                                    else if (parms.Length == 2)
                                    {
                                        var v1 = parms[0];
                                        var v2 = parms[0];

                                        movieClip.GoTo(v1.String, (int)v2.Integer);
                                    }
                                    movieClip.Play();
                                }
                                else if (funcName == "gotoAndStop")
                                {
                                    //stack.Pop();
                                    MovieClip movieClip = obj as MovieClip;
                                    if (movieClip == null)
                                    {
                                        movieClip = context.RootClip;
                                    }

                                    if (parms.Length == 1)
                                    {
                                        var v = parms[0];
                                        if (v.IsInteger)
                                            movieClip.GoTo((ushort)(v.Integer-1));
                                        else
                                            movieClip.GoTo(v.String);
                                    }
                                    else if (parms.Length == 2)
                                    {
                                        var v1 = parms[0];
                                        var v2 = parms[0];

                                        movieClip.GoTo(v1.String, (int)v2.Integer);
                                    }
                                    //movieClip.Stop();
                                 
                                }
                                else if (funcName == "getSeconds")
                                {
                                    stack.Pop();
                                    stack.Push(DateTime.Now.Second);
                                }
                                else if (funcName == "getMinutes")
                                {
                                    stack.Pop();
                                    stack.Push(DateTime.Now.Minute);
                                }
                                else if (funcName == "getHours")
                                {
                                    stack.Pop();
                                    stack.Push(DateTime.Now.Hour);
                                }
                                else if (funcName == "getMilliseconds")
                                {
                                    stack.Pop();
                                    stack.Push(DateTime.Now.Millisecond);
                                }
                                else if (funcName == "getBytesLoaded")
                                {
                                    stack.Pop();
                                    stack.Push(Loaded);
                                    if (Loaded >= Total)
                                        Loaded = 0;
                                    else
                                        Loaded++;
                                }
                                else if (funcName == "getBytesTotal")
                                {
                                    stack.Pop();
                                    stack.Push(Total);
                                }
                            }
                        }
                        break;
                    case ActionCode.ConstantPool:
                        pool = context.Constants = (_payloads[i] as ConstantPoolAction).Pool;
                        break;
                    case ActionCode.DefineFunction:
                    case ActionCode.DefineFunction2:
                        {
                            string funcName;
                            ActionFunc func;
                            if (_payloads[i] is DefineFunction2Action)
                            {
                                funcName = (_payloads[i] as DefineFunction2Action).Name;
                                func = new RuntimeActionFunc(context, _payloads[i] as DefineFunction2Action);
                            }
                            else
                            {
                                funcName = (_payloads[i] as DefineFunctionAction).Name;
                                func = new RuntimeActionFunc(context, _payloads[i] as DefineFunctionAction);
                            }

                            if (string.IsNullOrEmpty(funcName))
                                stack.Push(func);
                            else
                                SetVariable(target, context, funcName, func);
                        }
                        break;
                    case ActionCode.DefineLocal:
                        {
                            var value = stack.Pop();
                            var name = stack.Pop().String;
                            context.CurrentScope[name] = value;
                        }
                        break;
                    case ActionCode.DefineLocalUndef:
                        context.CurrentScope[stack.Pop().String] = new ActionVar();
                        break;
                    case ActionCode.Delete:
                        stack.Pop();
                        stack.Pop();
                        break;
                    case ActionCode.Delete2:
                        stack.Pop();
                        break;
                    case ActionCode.Enumerate:
                    case ActionCode.Enumerate2:
                        {
                            ActionObject obj = null;
                            Scope scope;

                            if (stack.Peek().IsObject)
                                obj = stack.Pop().Object;
                            else
                                obj = GetVariable(target, context, stack.Pop().String, out scope).Object;

                            stack.Push(new ActionVar((string)null));
                            if (obj == null) break;

                            foreach (var n in obj.VariableNames)
                                stack.Push(n);
                        }
                        break;
                    case ActionCode.Equals2:
                        stack.Push(stack.Pop().IsEqual(stack.Pop()));
                        break;
                    case ActionCode.GetMember:
                        {
                            var prop = stack.Pop();
                            var obj = stack.Pop().WrapperObject;

                            if (obj == null)
                            {
                                stack.Push(new ActionVar());
                                break;
                            }

                            stack.Push(prop.IsNumeric ? obj[(int)prop.Integer] : obj[prop.String]);
                        }
                        break;
                    case ActionCode.InitArray:
                        {
                            var vals = new ActionVar[stack.Pop().Integer];
                            for (int j = 0; j < vals.Length; j++)
                                vals[j] = stack.Pop();

                            stack.Push(new Objects.Array(vals));
                        }
                        break;
                    case ActionCode.InitObject:
                        {
                            var vals = new KeyValuePair<string, ActionVar>[stack.Pop().Integer];
                            ActionVar value;
                            for (int j = 0; j < vals.Length; j++)
                            {
                                value = stack.Pop();
                                vals[j] = new KeyValuePair<string, ActionVar>(stack.Pop().String, value);
                            }

                            stack.Push(new Objects.Object(vals));
                        }
                        break;
                    case ActionCode.NewMethod:
                        {
                            string name = stack.Pop().String;
                            var obj = stack.Pop().Object;
                            var vals = new ActionVar[stack.Pop().Integer];
                            for (int j = 0; j < vals.Length; j++)
                                vals[j] = stack.Pop();

                            if (obj == null)
                            {
                                stack.Push(new ActionVar());
                                break;
                            }

                            var func = string.IsNullOrEmpty(name) ? obj.Invokable : obj[name].Func;
                            stack.Push(func == null ? new ActionVar() : func.Invoke(obj.Context, vals));
                        }
                        break;
                    case ActionCode.NewObject:
                        {
                            string name = stack.Pop().String;
                            var vals = new ActionVar[stack.Pop().Integer];
                            for (int j = 0; j < vals.Length; j++)
                                vals[j] = stack.Pop();

                            stack.Push(context.GlobalScope.MakeInstance(name, vals));
                        }
                        break;
                    case ActionCode.SetMember:
                        {
                            var value = stack.Pop();
                            var prop = stack.Pop();
                            var obj = stack.Pop().WrapperObject;

                            if (obj == null)
                                break;

                            if (prop.IsNumeric)
                                obj[(int)prop.Integer] = value;
                            else
                                obj[prop.String] = value;

                        }
                        break;
                    case ActionCode.TargetPath:
                        {
                            var obj = stack.Pop().MovieClip;
                            stack.Push(obj != null ? obj.Path : new ActionVar());
                        }
                        break;
                    case ActionCode.With:
                        {
                            var payload = _payloads[i] as WithAction;
                            var obj = stack.Pop();

                            if (!obj.IsObject)
                                break;

                            context.Scope.AddLast(obj.Object);
                            payload.Actions.RunUnsafe(context);
                            context.Scope.RemoveLast();
                        }
                        break;
                    case ActionCode.ToNumber:
                        stack.Push(stack.Pop().Double);
                        break;
                    case ActionCode.ToString:
                        stack.Push(stack.Pop().String);
                        break;
                    case ActionCode.TypeOf:
                        stack.Push(stack.Pop().TypeOf);
                        break;
                    case ActionCode.Add2:
                        {
                            var A = stack.Pop();
                            var B = stack.Pop();
                            stack.Push((A.IsString || B.IsString) ? new ActionVar(B.String + A.String) : new ActionVar(B.Double + A.Double));
                        }
                        break;
                    case ActionCode.Less2:
                        {
                            var A = stack.Pop();
                            var B = stack.Pop();
                            stack.Push((A.IsString && B.IsString) ? B.String.CompareTo(A.String) < 0 : B.Double < A.Double);
                        }
                        break;
                    case ActionCode.Modulo:
                        {
                            var A = stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(A == 0 ? new ActionVar() : B % A);
                        }
                        break;
                    case ActionCode.BitAnd:
                        {
                            var A = stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(B & A);
                        }
                        break;
                    case ActionCode.BitOr:
                        {
                            var A = stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(B | A);
                        }
                        break;
                    case ActionCode.BitXor:
                        {
                            var A = stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(B ^ A);
                        }
                        break;
                    case ActionCode.BitLShift:
                        {
                            var A = (int)stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(B << A);
                        }
                        break;
                    case ActionCode.BitRShift:
                        {
                            var A = (int)stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push(B >> A);
                        }
                        break;
                    case ActionCode.BitURShift:
                        {
                            var A = (int)stack.Pop().Integer;
                            var B = stack.Pop().Integer;
                            stack.Push((long)((ulong)B >> A));
                        }
                        break;
                    case ActionCode.Increment:
                        stack.Push(stack.Pop().Double + 1);
                        break;
                    case ActionCode.Decrement:
                        stack.Push(stack.Pop().Double - 1);
                        break;
                    case ActionCode.PushDuplicate:
                        stack.Push(stack.Peek());
                        break;
                    case ActionCode.StackSwap:
                        {
                            var A = stack.Pop();
                            var B = stack.Pop();
                            stack.Push(A);
                            stack.Push(B);
                        }
                        break;
                    case ActionCode.StoreRegister:
                        context.Registers[(_payloads[i] as StoreRegisterAction).Register] = stack.Peek();
                        break;

                    #endregion

                    #region SWF 6

                    case ActionCode.StrictEquals:
                        stack.Push(stack.Pop().IsIdentical(stack.Pop()));
                        break;
                    case ActionCode.Greater:
                        {
                            var A = stack.Pop();
                            var B = stack.Pop();
                            stack.Push((A.IsString && B.IsString) ? B.String.CompareTo(A.String) > 0 : B.Double > A.Double);
                        }
                        break;
                    case ActionCode.StringGreater:
                        {
                            var A = stack.Pop().String;
                            var B = stack.Pop().String;
                            stack.Push(B.CompareTo(A) > 0);
                        }
                        break;

                    #endregion

                    #region SWF 7

                    case ActionCode.Extends:
                        {
                            var super = stack.Pop().Func;
                            var sub = stack.Pop().Func;

                            var obj = new Objects.Object(super.Prototype, super);
                            sub.Prototype = obj;
                        }
                        break;
                    case ActionCode.Throw:
                        throw new RuntimeException(stack.Pop());
                    case ActionCode.Try:
                        {
                            var payload = _payloads[i] as TryAction;
                            try
                            {
                                payload.Try.RunUnsafe(context);
                            }
                            catch (RuntimeException e)
                            {
                                if (payload.CatchRegister.HasValue)
                                    context.Registers[payload.CatchRegister.Value] = e.Value;
                                if (!string.IsNullOrEmpty(payload.CatchVariable))
                                    context.CurrentScope[payload.CatchVariable] = e.Value;
                                if (payload.Catch != null)
                                    payload.Catch.RunUnsafe(context);
                            }
                            finally
                            {
                                if (payload.Finally != null)
                                    payload.Finally.RunUnsafe(context);
                            }
                        }
                        break;

                    #endregion

                    #region Ignored actions

                    // Ignored because networking not supported
                    case ActionCode.GetURL:
                        context.RootClip.Trace("GetURL is not supported and will be ignored!");
                        break;
                    case ActionCode.GetURL2:
                        context.RootClip.Trace("GetURL2 is not supported and will be ignored!");
                        stack.Pop();
                        stack.Pop();
                        break;

                    // Ignored because clip is loaded at once 
                    case ActionCode.WaitForFrame:
                        break;
                    case ActionCode.WaitForFrame2:
                        stack.Pop();
                        break;

                    #endregion

                    case ActionCode.CastOp:
                    case ActionCode.InstanceOf:
                    case ActionCode.ImplementsOp:
                    default:
                        throw new InvalidOperationException("Unimplemented action '" + _actions[i].ToString() + "'found!");
                }
            }

            return new ActionVar();
#endif
        }

        private class RuntimeException : Exception
        {
            public ActionVar Value { get; private set; }
            public RuntimeException(ActionVar value) { Value = value; }
        }
    }
}
