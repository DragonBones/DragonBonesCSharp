using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DragonBones
{
    /**
     * @private
     * @internal
     */
    internal abstract class Constraint : BaseObject
    {
        protected static readonly Matrix _helpMatrix = new Matrix();
        protected static readonly Transform _helpTransform = new Transform();
        protected static readonly Point _helpPoint = new Point();

        public Bone target;
        public Bone bone;
        public Bone root;

        protected override void _OnClear()
        {
            this.target = null; //
            this.bone = null; //
            this.root = null; //
        }

        public abstract void Update();
    }
    /**
     * @private
     * @internal
     */
    internal class IKConstraint : Constraint
    {
        public bool bendPositive;
        public bool scaleEnabled; // TODO
        public float weight;

        protected override void _OnClear()
        {
            base._OnClear();

            this.bendPositive = false;
            this.scaleEnabled = false;
            this.weight = 1.0f;
        }

        private void _ComputeA()
        {
            var ikGlobal = this.target.global;
            var global = this.bone.global;
            var globalTransformMatrix = this.bone.globalTransformMatrix;
            // const boneLength = this.bone.boneData.length;
            // const x = globalTransformMatrix.a * boneLength; 

            var ikRadian = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                ikRadian += (float)Math.PI;
            }

            global.rotation += (ikRadian - global.rotation) * this.weight;
            global.ToMatrix(globalTransformMatrix);
        }

        private void _ComputeB()
        {
            var boneLength = this.bone.boneData.length;
            var parent = this.root as Bone;
            var ikGlobal = this.target.global;
            var parentGlobal = parent.global;
            var global = this.bone.global;
            var globalTransformMatrix = this.bone.globalTransformMatrix;

            var x = globalTransformMatrix.a * boneLength;
            var y = globalTransformMatrix.b * boneLength;

            var lLL = x * x + y * y;
            var lL = (float)Math.Sqrt(lLL);

            var dX = global.x - parentGlobal.x;
            var dY = global.y - parentGlobal.y;
            var lPP = dX * dX + dY * dY;
            var lP = (float)Math.Sqrt(lPP);
            var rawRadianA = (float)Math.Atan2(dY, dX);

            dX = ikGlobal.x - parentGlobal.x;
            dY = ikGlobal.y - parentGlobal.y;
            var lTT = dX * dX + dY * dY;
            var lT = (float)Math.Sqrt(lTT);

            var ikRadianA = 0.0f;
            if (lL + lP <= lT || lT + lL <= lP || lT + lP <= lL)
            {
                ikRadianA = (float)Math.Atan2(ikGlobal.y - parentGlobal.y, ikGlobal.x - parentGlobal.x);
                if (lL + lP <= lT)
                {
                }
                else if (lP < lL)
                {
                    ikRadianA += (float)Math.PI;
                }
            }
            else
            {
                var h = (lPP - lLL + lTT) / (2.0f * lTT);
                var r = (float)Math.Sqrt(lPP - h * h * lTT) / lT;
                var hX = parentGlobal.x + (dX * h);
                var hY = parentGlobal.y + (dY * h);
                var rX = -dY * r;
                var rY = dX * r;

                var isPPR = false;
                if (parent._parent != null)
                {
                    var parentParentMatrix = parent._parent.globalTransformMatrix;
                    isPPR = parentParentMatrix.a * parentParentMatrix.d - parentParentMatrix.b * parentParentMatrix.c < 0.0f;
                }

                if (isPPR != this.bendPositive)
                {
                    global.x = hX - rX;
                    global.y = hY - rY;
                }
                else
                {
                    global.x = hX + rX;
                    global.y = hY + rY;
                }

                ikRadianA = (float)Math.Atan2(global.y - parentGlobal.y, global.x - parentGlobal.x);
            }

            var dR = (ikRadianA - rawRadianA) * this.weight;
            parentGlobal.rotation += dR;
            parentGlobal.ToMatrix(parent.globalTransformMatrix);

            var parentRadian = rawRadianA + dR;
            global.x = parentGlobal.x + (float)Math.Cos(parentRadian) * lP;
            global.y = parentGlobal.y + (float)Math.Sin(parentRadian) * lP;

            var ikRadianB = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                ikRadianB += (float)Math.PI;
            }

            dR = (ikRadianB - global.rotation) * this.weight;
            global.rotation += dR;
            global.ToMatrix(globalTransformMatrix);
        }

        public override void Update()
        {
            if (this.root == null)
            {
                this.bone.UpdateByConstraint();
                this._ComputeA();
            }
            else {
            this.root.UpdateByConstraint();
            this.bone.UpdateByConstraint();
            this._ComputeB();
        }
        }
    }
}
