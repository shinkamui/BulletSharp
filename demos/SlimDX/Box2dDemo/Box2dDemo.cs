﻿using System;
using System.Drawing;
using System.Windows.Forms;
using BulletSharp;
using DemoFramework;
using SlimDX;
using SlimDX.Direct3D9;

namespace Box2dDemo
{
    public class Box2dDemo : Game
    {
        Vector3 eye = new Vector3(0, 15, 20);
        Vector3 target = new Vector3(10, 10, 0);

        Light light;

        protected override void OnInitializeDevice()
        {
            Form.Text = "BulletSharp - Box2D Demo";
            base.OnInitializeDevice();
        }

        protected override void OnInitialize()
        {
            PhysicsContext = new Physics();

            light = new Light();
            light.Type = LightType.Point;
            light.Range = 250;
            light.Position = new Vector3(10, 25, 10);
            light.Diffuse = Color.LemonChiffon;
            light.Attenuation0 = 1.0f;

            Freelook.SetEyeTarget(eye, target);

            Fps.Text = "Move using mouse and WASD+shift\n" +
                "F3 - Toggle debug\n" +
                "F11 - Toggle fullscreen\n" +
                "Space - Shoot box";

            base.OnInitialize();
        }

        protected override void OnResetDevice()
        {
            base.OnResetDevice();

            Device.SetLight(0, light);
            Device.EnableLight(0, true);

            Device.SetRenderState(RenderState.CullMode, Cull.None);
        }

        protected override void OnRender()
        {
            Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.LightGray, 1.0f, 0);
            Device.BeginScene();

            Device.SetTransform(TransformState.View, Freelook.View);

            foreach (RigidBody body in PhysicsContext.World.CollisionObjectArray)
            {
                Device.SetTransform(TransformState.World, body.MotionState.WorldTransform);
                RenderWithMaterial(body);
            }

            DebugDrawWorld();

            Fps.OnRender(FramesPerSecond);

            Device.EndScene();
            Device.Present();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (LibraryTest.Test() == false)
                return;

            RunGame();
        }

        static void RunGame()
        {
            using (Box2dDemo game = new Box2dDemo())
            {
                game.Run();
            }
        }
    }
}
