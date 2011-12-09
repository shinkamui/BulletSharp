﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace DemoFramework
{
    public class Graphics
    {
        public Demo Demo
        {
            get;
            protected set;
        }

        public Form Form
        {
            get;
            protected set;
        }

        public float FieldOfView
        {
            get;
            protected set;
        }

        public float AspectRatio
        {
            get
            {
                Size clientSize = Form.ClientSize;
                return (float)clientSize.Width / (float)clientSize.Height;
            }
        }

        public Graphics(Demo demo)
        {
            Demo = demo;
            FieldOfView = (float)Math.PI / 4;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Run()
        {
        }

        public virtual void UpdateView()
        {
        }

        public virtual void SetFormText(string text)
        {
            Form.Text = text;
        }

        public virtual void SetInfoText(string text)
        {
        }
    }
}