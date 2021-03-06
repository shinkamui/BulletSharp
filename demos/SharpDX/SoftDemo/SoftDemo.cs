﻿using System;
using System.Drawing;
using System.Windows.Forms;
using BulletSharp;
using BulletSharp.SoftBody;
using DemoFramework;
using SharpDX;
using Face = BulletSharp.SoftBody.Face;
using Point = System.Drawing.Point;

namespace SoftDemo
{
    public class SoftDemo : Demo
    {
        Vector3 eye = new Vector3(20, 20, 80);
        Vector3 target = new Vector3(0, 0, 10);

        Point lastMousePos;
        Vector3 impact;
        SRayCast results = new SRayCast();
        Node node;
        Vector3 goal;
        bool drag;

        protected override void OnInitializeDevice()
        {
            Form.Text = "BulletSharp - SoftBody Demo";
            base.OnInitializeDevice();
        }

        protected override void OnInitialize()
        {
            PhysicsContext = new Physics();
            PhysicsContext.World.SetInternalTickCallback(PickingPreTickCallback, this, true);

            Info.Text = "Move using mouse and WASD+shift\n" +
                //"F3 - Toggle debug\n" +
                //"F11 - Toggle fullscreen\n" +
                "Space - Shoot box\n\n" +
                "B - Previous Demo\n" +
                "N - Next Demo";

            Freelook.SetEyeTarget(eye, target);

            base.OnInitialize();
        }

        class ImplicitSphere : ImplicitFn
        {
            Vector3 center;
            float sqradius;

            public ImplicitSphere(Vector3 c, float r)
            {
                center = c;
                sqradius = r * r;
            }
            public override float Eval(Vector3 x)
            {
                return ((x - center).LengthSquared() - sqradius);
            }
        };

        void PickingPreTickCallback(DynamicsWorld world, float timeStep)
        {
            if (drag)
            {
                Vector3 rayFrom = Freelook.Eye;
                Vector3 rayTo = GetRayTo(lastMousePos, Freelook.Eye, Freelook.Target, FieldOfView);
                Vector3 rayDir = (rayTo - rayFrom);
                rayDir.Normalize();
                Vector3 N = (Freelook.Target - Freelook.Eye);
                N.Normalize();
                float O = Vector3.Dot(impact, N);
                float den = Vector3.Dot(N, rayDir);
                if ((den * den) > 0)
                {
                    float num = O - Vector3.Dot(N, rayFrom);
                    float hit = num / den;
                    if ((hit > 0) && (hit < 1500))
                    {
                        goal = rayFrom + rayDir * hit;
                    }
                }
                Vector3 delta = goal - node.X;
                float maxdrag = 10;
                if (delta.LengthSquared() > (maxdrag * maxdrag))
                {
                    delta.Normalize();
                    delta *= maxdrag;
                }
                node.Velocity += delta / timeStep;
            }
        }

        protected override void OnHandleInput()
        {
            base.OnHandleInput();

            if (Input.MousePressed == MouseButtons.Right)
            {
                results.Fraction = 1;
                if (pickConstraint == null)
                {
                    Vector3 rayFrom = Freelook.Eye;
                    Vector3 rayTo = GetRayTo(Input.MousePoint, Freelook.Eye, Freelook.Target, FieldOfView);
                    Vector3 rayDir = (rayTo - rayFrom);
                    rayDir.Normalize();
                    AlignedSoftBodyArray sbs = ((SoftRigidDynamicsWorld)PhysicsContext.World).SoftBodyArray;
                    for (int ib = 0; ib < sbs.Count; ++ib)
                    {
                        SoftBody psb = sbs[ib];
                        SRayCast res = new SRayCast();
                        if (psb.RayTest(rayFrom, rayTo, res))
                        {
                            results = res;
                        }
                    }
                    if (results.Fraction < 1)
                    {
                        impact = rayFrom + (rayTo - rayFrom) * results.Fraction;
                        drag = !(PhysicsContext as Physics).cutting;
                        lastMousePos = Input.MousePoint;
                        node = null;
                        switch (results.Feature)
                        {
                            case EFeature.Tetra:
                                {
                                    Tetra tet = results.Body.Tetras[results.Index];
                                    node = tet.Nodes[0];
                                    for (int i = 1; i < 4; ++i)
                                    {
                                        if ((node.X - impact).LengthSquared() >
                                            (tet.Nodes[i].X - impact).LengthSquared())
                                        {
                                            node = tet.Nodes[i];
                                        }
                                    }
                                    break;
                                }
                            case EFeature.Face:
                                {
                                    Face f = results.Body.Faces[results.Index];
                                    node = f.N[0];
                                    for (int i = 1; i < 3; ++i)
                                    {
                                        if ((node.X - impact).LengthSquared() >
                                            (f.N[i].X - impact).LengthSquared())
                                        {
                                            node = f.N[i];
                                        }
                                    }
                                }
                                break;
                        }
                        if (node != null)
                            goal = node.X;
                        //return;
                    }
                }
            }
            else if (Input.MouseReleased == MouseButtons.Right)
            {
                if ((!drag) && (PhysicsContext as Physics).cutting && (results.Fraction < 1))
                {
                    ImplicitSphere isphere = new ImplicitSphere(impact, 1);
                    results.Body.Refine(isphere, 0.0001f, true);
                }
                results.Fraction = 1;
                drag = false;
            }

            // Mouse movement
            if (Input.MouseDown == MouseButtons.Right)
            {
                if (node != null && (results.Fraction < 1))
                {
                    if (!drag)
                    {
                        int x = Input.MousePoint.X - lastMousePos.X;
                        int y = Input.MousePoint.Y - lastMousePos.Y;
                        if ((x * x) + (y * y) > 6)
                        {
                            drag = true;
                        }
                    }
                    if (drag)
                    {
                        lastMousePos = Input.MousePoint;
                    }
                }
            }

            (PhysicsContext as Physics).HandleInput(Input, FrameDelta);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            if (LibraryTest.Test() == false)
                return;

            RunDemo();
        }

        static void RunDemo()
        {
            using (Demo demo = new SoftDemo())
            {
                demo.Run();
            }
        }
    }
}
