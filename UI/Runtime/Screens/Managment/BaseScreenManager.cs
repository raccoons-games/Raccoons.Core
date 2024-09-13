using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Raccoons.UI.Guids;

namespace Raccoons.UI.Screens
{
    public abstract class BaseScreenManager : BaseScreenManager<BaseScreen> { }
    public abstract class BaseScreenManager<TScreen> : MonoBehaviour where TScreen : BaseScreen
    {
        [SerializeField]
        private List<TScreen> _allScreens;

        [Header("ReadOnly")]
        [SerializeField]
        private List<TScreen> _allOpenedScreens;

        public event EventHandler<IScreen> OnOpenedScreen;
        public event EventHandler<IScreen> OnClosedScreen;

        protected List<TScreen> AllOpenedScreens { get => _allOpenedScreens; }
        protected List<TScreen> AllScreens { get => _allScreens; }

        #region Awake
        protected virtual void Awake()
        {
            _allScreens.ForEach(
                screen =>
                {
                    screen.OnOpened += Screen_OnOpened;
                    screen.OnClosed += Screen_OnClosed;
                });
            _allOpenedScreens = new List<TScreen>();
            _allOpenedScreens = _allScreens.FindAll(x => x.IsOpen() == true);
        }
        #endregion

        #region Open / Close
        protected virtual void ShowScreen(IScreen screen)
        {
            if (screen != null)
            {
                screen.Open();
            }
            else
            {
                Debug.LogError($"{gameObject.name} Screen == null!");
            }
        }
        public virtual void ShowScreen<R>() where R: BaseScreen
        {
            R screen = GetScreen<R>(typeof(R));
            ShowScreen(screen);
        }
        protected virtual void CloseScreen(IScreen screen)
        {
            if (screen != null)
            {
                screen.Close();
            }
        }

        public virtual void CloseAllScreens()
        {
            List<BaseScreen> temp = new List<BaseScreen>(_allOpenedScreens);
            temp?.ForEach(screen => CloseScreen(screen));
        }
        #endregion

        #region Opened screens
        private void AddOpenedScreen(IScreen screen)
        {
            TScreen baseScreen = screen as TScreen;
            if (screen != null)
            {
                if (_allOpenedScreens.Contains(baseScreen) == false)
                {
                    _allOpenedScreens.Add(baseScreen);
                }
            }
        }
        private void RemoveOpenedScreen(IScreen screen)
        {
            TScreen baseScreen = screen as TScreen;
            if (screen != null)
            {
                if (_allOpenedScreens.Contains(baseScreen) == true)
                {
                    _allOpenedScreens.Remove(baseScreen);
                }
            }
        }
        #endregion

        #region Get
        protected RScreen GetScreen<RScreen>(Type type) where RScreen : BaseScreen
        {
            RScreen result = _allScreens.Find(x => x.GetType() == type) as RScreen;
         //   R result = allScreens.Find(x => x.GetType() == typeof(R)) as R;
            return result;
        }
        protected RScreen GetScreen<RScreen>(string key) where RScreen : BaseScreen
        {
            List<TScreen> screens = _allScreens.FindAll(x => x.GetType() is RScreen);
            RScreen result = screens.Find(x => x.Key == key) as RScreen;
            return result;
        }
        protected RScreen GetScreen<RScreen>() where RScreen : BaseScreen
        {
            return GetScreen<RScreen>(typeof(RScreen));
        }
        protected RScreen GetScreen<RScreen>(IUIGuid guid) where RScreen : BaseScreen
        {
            RScreen result = null;
            if (guid != null)
            {
                List<TScreen> screens = _allScreens.FindAll(x => x.Guid == guid);
                result = screens.Find(x => x.GetType() == typeof(RScreen)) as RScreen;
            }
            return result;
        }

        #endregion

            #region Events
        protected virtual void Screen_OnClosed(object sender, IScreen e)
        {
            RemoveOpenedScreen(e);
            OnClosedScreen?.Invoke(this, e);
        }

        protected virtual void Screen_OnOpened(object sender, IScreen e)
        {
            AddOpenedScreen(e);
            OnOpenedScreen?.Invoke(this, e);
        }
        #endregion

        #region OnDestroy
        protected virtual void OnDestroy()
        {
            _allScreens.ForEach(
               screen =>
               {
                   screen.OnOpened -= Screen_OnOpened;
                   screen.OnClosed -= Screen_OnClosed;
               });
        }
        #endregion
    }
}