using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using dove;

namespace Game
{
    [GameSystem(-100)]
    public class EventSystem : IGameSystem
    {
        public GameMain Game { get; set; }
        private Dictionary<GameEventType, Action<object>> actions = new Dictionary<GameEventType, Action<object>>();

        private Dictionary<object, List<KeyValuePair<GameEventType, Action<object>>>> registeredActions = 
            new Dictionary<object, List<KeyValuePair<GameEventType, Action<object>>>>();

        public static void sEmitEvent(GameEventType evtType, object param) {
            GameMain.GetSystem<EventSystem>()?.EmitEvent(evtType, param);
        }

        public void RegEvents(object obj) {
            var type = obj.GetType();
            var methods = type.GetMethods();
            if (registeredActions.ContainsKey(obj)) return;
            var actionList = new List<KeyValuePair<GameEventType, Action<object>>>();
            foreach (var method in methods) {
                var attribute = method.GetCustomAttribute(typeof(GameEventHandler));
                if (attribute != null && !method.IsStatic) {
                    GameEventType evtType = (attribute as GameEventHandler).evtType;
                    Action<object> action = method.CreateDelegate(typeof(Action<object>), obj) as Action<object>;
                    if (actions.ContainsKey(evtType)) actions[evtType] += action;
                    else actions.Add(evtType, action);
                    actionList.Add(new KeyValuePair<GameEventType, Action<object>>(evtType, action));
                }
            }
            registeredActions.Add(obj, actionList);
        }
        public void UnregEvents(object obj) {
            if (!registeredActions.ContainsKey(obj)) return;
            foreach (var action in registeredActions[obj]) {
                if (!actions.ContainsKey(action.Key)) continue;
                actions[action.Key] -= action.Value;
            }
            registeredActions.Remove(obj);
        }

        public void EmitEvent(GameEventType evtType, object param) {
            if (actions.ContainsKey(evtType))
                actions[evtType]?.Invoke(param);
        }

        public void OnInit() {
            Game.RegCallback(GameMain.CallbackType.AllSystemInited, () => { EmitEvent(GameEventType.GameSystemsInited, null); });
        }

        public void OnRelease() {
            actions?.Clear();
        }
    }

    public enum GameEventType
    { 
        GameSystemsInited,
        GameStart,
        GameRelease
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class GameEventHandler : Attribute
    {
        public GameEventType evtType;
        public Type paramType = null;

        public GameEventHandler(GameEventType type) {
            evtType = type;
        }
    }
}
