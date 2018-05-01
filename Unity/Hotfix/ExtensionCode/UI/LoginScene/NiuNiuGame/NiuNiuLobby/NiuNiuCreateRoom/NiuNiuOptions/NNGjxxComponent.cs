﻿using System;
using System.Collections.Generic;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class NNGjxxComponentAwakeSystem : AwakeSystem<NNGjxxComponent>
    {
        public override void Awake(NNGjxxComponent self)
        {
            self.Awake();
        }
    }

    [ObjectSystem]
    public class NNGjxxComponentStartSystem : StartSystem<NNGjxxComponent>
    {
        public override void Start(NNGjxxComponent self)
        {
            self.Start();
        }
    }

    public class NNGjxxComponent : Component
    {
        private GameObject _osbPanel;

        private GameObject nnOptionItemPrefab;

        private GameObject optionShowBoxGrid;

        private GameObject nnOptionShowBox;

        private GameObject allSelectBtn;

        private GameObject nnGaoJiXuanXiangDp;

        // 勾选的选项索引集合
        public List<int> SelectedOptions;

        private GameObject SelectedText;

        private UI NNTspx;
        private UI NNCreateRoom;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            nnOptionShowBox = rc.Get<GameObject>("NiuNiuGjxx");
            nnGaoJiXuanXiangDp = rc.Get<GameObject>("GaoJiXuanXiangDp");
            _osbPanel = rc.Get<GameObject>("OptionShowBoxPanel");

            allSelectBtn = rc.Get<GameObject>("AllSelectBtn");
            optionShowBoxGrid = rc.Get<GameObject>("OptionShowBoxGrid");

            nnOptionItemPrefab = rc.Get<GameObject>("NN_OptionItem");

            SelectedText = nnGaoJiXuanXiangDp.transform.Find("Label").gameObject;

            SelectedOptions = new List<int>();

            SceneHelperComponent.Instance.MonoEvent.AddButtonClick(nnOptionShowBox.GetComponent<Button>(), CloseMask);
            SceneHelperComponent.Instance.MonoEvent.AddButtonClick(nnGaoJiXuanXiangDp.GetComponent<Button>(), () =>
            {
                NNTspx.GetComponent<NNTspxComponent>().CloseMask();
                _osbPanel.SetActive(true);
                nnOptionShowBox.GetComponent<Image>().raycastTarget = true;
                NNCreateRoom.GetComponent<NiuNiuCRComponent>().LockCreateBtn();
            });
            SceneHelperComponent.Instance.MonoEvent.AddButtonClick(allSelectBtn.GetComponent<Button>(), () =>
            {
                if (allSelectBtn.transform.Find("UnSelected").gameObject.activeSelf)
                {
                    SelectedAllOptions();
                }
                else
                {
                    ClearAllOptions();
                }
            });
        }

        public void Start()
        {
            NNTspx = Game.Scene.GetComponent<UIComponent>().Get(UIType.NiuNiuTspx);
            NNCreateRoom = Game.Scene.GetComponent<UIComponent>().Get(UIType.NiuNiuCreateRoom);

            for (int i = 0; i < NiuNiuRuleInstance.ZiYouQiangZhuang.ListGaoJiXuanXiang.Count; i++)
            {
                GameObject gameobject = UnityEngine.Object.Instantiate(nnOptionItemPrefab, optionShowBoxGrid.transform);
                gameobject.transform.Find("Text").GetComponent<Text>().text = NiuNiuRuleInstance.ZiYouQiangZhuang.ListGaoJiXuanXiang[i];
                gameobject.name = i.ToString();
                _osbPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1124f, 350f);
                Vector2 tempPos = nnGaoJiXuanXiangDp.transform.Find("Row2Pos").GetComponent<RectTransform>().anchoredPosition;
                _osbPanel.GetComponent<RectTransform>().anchoredPosition = tempPos;

                SceneHelperComponent.Instance.MonoEvent.AddButtonClick(gameobject.GetComponent<Button>(), () =>
                {
                    if (gameobject.transform.Find("UnSelected").gameObject.activeSelf)
                    {
                        gameobject.transform.Find("UnSelected").gameObject.SetActive(false);
                        gameobject.transform.Find("Selected").gameObject.SetActive(true);
                        SelectedOptions.Add(Convert.ToInt32(gameobject.name));
                        RefreshOptionsText();
                    }
                    else
                    {
                        gameobject.transform.Find("UnSelected").gameObject.SetActive(true);
                        gameobject.transform.Find("Selected").gameObject.SetActive(false);
                        SelectedOptions.Remove(Convert.ToInt32(gameobject.name));
                        RefreshOptionsText();
                    }
                });
            }


            RefreshShowItem(NiuNiuRuleInstance.NiuNiuShangZhuang.ListGaoJiXuanXiang.Count);
        }

        public void CloseMask()
        {
            _osbPanel.SetActive(false);
            nnOptionShowBox.GetComponent<Image>().raycastTarget = false;
            NNCreateRoom.GetComponent<NiuNiuCRComponent>().UnLockCreateBtn();
        }


        public void RefreshShowItem(int ruleCount)
        {
            // 清除额外勾选选项
            ClearAllOptions();

            if (ruleCount <= 3)
            {
                _osbPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1124f, 187f);
                Vector2 tempPos = nnGaoJiXuanXiangDp.transform.Find("Row1Pos").GetComponent<RectTransform>().anchoredPosition;
                _osbPanel.GetComponent<RectTransform>().anchoredPosition = tempPos;
            }
            else if (ruleCount > 3 && ruleCount <= 6)
            {
                _osbPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1124f, 269f);
                Vector2 tempPos = nnGaoJiXuanXiangDp.transform.Find("Row2Pos").GetComponent<RectTransform>().anchoredPosition;
                _osbPanel.GetComponent<RectTransform>().anchoredPosition = tempPos;
            }
            else
            {
                _osbPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1124f, 350f);
                Vector2 tempPos = nnGaoJiXuanXiangDp.transform.Find("Row3Pos").GetComponent<RectTransform>().anchoredPosition;
                _osbPanel.GetComponent<RectTransform>().anchoredPosition = tempPos;
            }

            for (int i = 0; i < optionShowBoxGrid.transform.childCount; i++)
            {
                optionShowBoxGrid.transform.GetChild(i).gameObject.SetActive(i < ruleCount);
            }


            foreach (var item in SelectedOptions)
            {
                Debug.Log("高级选项" + item);
            }
        }


        private void ClearAllOptions()
        {
            allSelectBtn.transform.Find("Selected").gameObject.SetActive(false);
            allSelectBtn.transform.Find("UnSelected").gameObject.SetActive(true);
            int itemIndex = 0;
            foreach (Transform item in optionShowBoxGrid.transform)
            {
                item.Find("UnSelected").gameObject.SetActive(true);
                item.Find("Selected").gameObject.SetActive(false);
                if (SelectedOptions.Contains(itemIndex))
                {
                    SelectedOptions.Remove(itemIndex);
                }

                itemIndex++;
            }

            SelectedText.GetComponent<Text>().text = "无";
        }

        private void SelectedAllOptions()
        {
            allSelectBtn.transform.Find("UnSelected").gameObject.SetActive(false);
            allSelectBtn.transform.Find("Selected").gameObject.SetActive(true);
            int itemIndex = 0;
            foreach (Transform item in optionShowBoxGrid.transform)
            {
                item.Find("UnSelected").gameObject.SetActive(false);
                item.Find("Selected").gameObject.SetActive(true);
                if (!SelectedOptions.Contains(itemIndex))
                {
                    SelectedOptions.Add(itemIndex);
                }

                itemIndex++;
            }

            RefreshOptionsText();
        }

        private void RefreshOptionsText()
        {
            SelectedText.GetComponent<Text>().text = "";

            int count = 0;

            foreach (Transform select in optionShowBoxGrid.transform)
            {
                if (select.Find("Selected").gameObject.activeSelf)
                {
                    SelectedText.GetComponent<Text>().text += select.Find("Text").GetComponent<Text>().text + "    ";
                    count++;
                }
            }

            if (count == 0)
            {
                SelectedText.GetComponent<Text>().text = "无";
            }
        }
    }
}