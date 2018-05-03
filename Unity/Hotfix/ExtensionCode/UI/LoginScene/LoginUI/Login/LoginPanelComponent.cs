﻿using System;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class LoginPanelComponentAwakeSystem : AwakeSystem<LoginPanelComponent>
    {
        public override void Awake(LoginPanelComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class LoginPanelComponentStartSystem : StartSystem<LoginPanelComponent>
    {
        public override void Start(LoginPanelComponent self)
        {
            self.Start();
        }
    }


    public class LoginPanelComponent : Component
    {
        private UI _dialogPanelUI;
        private UI _registPanelUI;
        private GameObject loginSubmitBtn;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            // 登录窗口
            var loginPanel = rc.Get<GameObject>("LoginPanel");
            var loginCloseBtn = rc.Get<GameObject>("LoginCloseBtn");
            var loginNameInputField = rc.Get<GameObject>("LoginNameInputField");
            var loginPwdInputField = rc.Get<GameObject>("LoginPwdInputField");
            loginSubmitBtn = rc.Get<GameObject>("LoginSubmitBtn");

            // 关闭登录窗口按钮
            SceneHelperComponent.Instance.MonoEvent.AddButtonClick(loginCloseBtn.GetComponent<Button>(),
                () =>
                {
                    loginPanel.SetActive(false);
                    loginNameInputField.GetComponent<InputField>().text = "";
                    loginPwdInputField.GetComponent<InputField>().text = "";
                });

            // 登录帐号按钮
            SceneHelperComponent.Instance.MonoEvent.AddButtonClick(loginSubmitBtn.GetComponent<Button>(),
                () => OnLoginSubmitBtn(loginNameInputField, loginPwdInputField));
        }

        public void Start()
        {
            _dialogPanelUI = Game.Scene.GetComponent<UIComponent>().Get(UIType.DialogPanel);
            _registPanelUI = Game.Scene.GetComponent<UIComponent>().Get(UIType.RegistPanel);
        }

        /// <summary>
        /// 登录方法
        /// </summary>
        /// <param name="loginNameText"></param>
        /// <param name="loginPwdText"></param>
        public async void OnLoginSubmitBtn(GameObject loginNameText, GameObject loginPwdText)
        {
            if (String.IsNullOrWhiteSpace(loginPwdText.GetComponent<InputField>().text) ||
                String.IsNullOrWhiteSpace(loginNameText.GetComponent<InputField>().text))
            {
                _dialogPanelUI.GetComponent<DialogPanelComponent>().ShowDialogBox("帐号或密码不能为空!");
                return;
            }

            try
            {
                SessionWrap session = _registPanelUI.GetComponent<RegistPanelComponent>().Session;

                if (session == null)
                {
                    session = SceneHelperComponent.Instance.CreateRealmSession();
                }

                SceneHelperComponent.Instance.MonoEvent.RemoveButtonClick(loginSubmitBtn.GetComponent<Button>());

                LoginResponse response = (LoginResponse) await session.Call(
                    new LoginRequest()
                    {
                        UserName = loginNameText.GetComponent<InputField>().text,
                        Password = loginPwdText.GetComponent<InputField>().text
                    });

                if (response.Error == 0)
                {
                    session.Dispose();

                    // 连接网关服务器

                    await SceneHelperComponent.Instance.CreateGateSession(response.Address, response.Key);

                    
                    // 获取用户信息
                    var accountResponse = (GetAccountInfoResponse) await SceneHelperComponent.Instance.Session.Call(new GetAccountInfoRequest());
//                    accountResponse.AccountInfo
                    
                    Game.Scene.GetComponent<UIComponent>().Create(UIType.Lobby, UiLayer.Bottom);

                    Game.Scene.GetComponent<UIComponent>().Remove(UIType.Login);
                }
                else if (response.Error == -1)
                {
                    // 登录失败
                    _dialogPanelUI.GetComponent<DialogPanelComponent>().ShowDialogBox(response.Message);
                }

                SceneHelperComponent.Instance.MonoEvent.AddButtonClick(loginSubmitBtn.GetComponent<Button>(),
                    () => OnLoginSubmitBtn(loginNameText, loginPwdText));
            }
            catch (Exception e)
            {
                _dialogPanelUI.GetComponent<DialogPanelComponent>().ShowDialogBox("网络连接错误:" + e.Message);
            }
        }
        
        /// <summary>
        /// 加入房间
        /// </summary>
        private async void JoinPaiJu(long roomId)
        {
            // TODO 加入牌局
            var joinRoomResponse = (JoinRoomResponse) await SceneHelperComponent.Instance.Session.Call(
                new JoinRoomRequest() {RoomId = roomId});
            
            if (joinRoomResponse.Error == 0)
            {
                Debug.Log("加入房间成功,跳转至游戏主场景");

                Game.Scene.GetComponent<UIComponent>().Create(UIType.NiuNiuMain, UiLayer.Bottom, roomId);
                Game.Scene.GetComponent<UIComponent>().Remove(UIType.NiuNiuLobby);
            }
            else
            {
                Debug.Log("加入房间失败: " + joinRoomResponse.Message);
            }
        }
    }
}