using Configgy;
using Configgy.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CarcassEnemy
{
    public class UIMeme : IConfigElement
    {
        private static Sprite funneImage;

        private static Sprite GetFunneImage()
        {
            if (funneImage == null)
            {
                funneImage = Plugin.AssetLoader.LoadAsset<Sprite>("ApplyParams");
            }
            return funneImage;
        }

        public void BindDescriptor(Configgy.Configgable configgable)
        {
            this.descriptor = configgable;
        }

        public void BuildElement(RectTransform rect)
        {
            DynUI.Frame(rect, (f) =>
            {
                Vector2 size = f.RectTransform.sizeDelta;

                GameObject imageObject = new GameObject("Funny Image");
                imageObject.transform.parent = f.Content;
                RectTransform rt = imageObject.AddComponent<RectTransform>();
                Image img = imageObject.AddComponent<Image>();
                Button b = imageObject.AddComponent<Button>();
                b.onClick.AddListener(CarcassCFG.ApplyParameters);
                Sprite sprite = GetFunneImage();
                size.y = sprite.texture.height;
                f.RectTransform.sizeDelta = size;
                DynUI.Layout.FillParent(rt);
                img.sprite = sprite;
            });
        }

        private Configgy.Configgable descriptor;

        public Configgy.Configgable GetDescriptor()
        {
            return descriptor;
        }

        public void OnMenuClose()
        {

        }


        public void OnMenuOpen()
        {

        }
    }
}
