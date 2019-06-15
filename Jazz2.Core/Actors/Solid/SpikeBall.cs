﻿using System.Threading.Tasks;
using Duality;
using Duality.Components;
using Duality.Drawing;
using Jazz2.Actors.Enemies;
using Jazz2.Game;

namespace Jazz2.Actors.Solid
{
    public class SpikeBall : EnemyBase
    {
        private const float BaseCycleFrames = 800f;

        private float speed;
        private int length;
        private bool isSwing, shade;

        private float phase;
        private Vector3 originPos;
        private ChainPiece[] pieces;

        protected override async Task OnActivatedAsync(ActorActivationDetails details)
        {
            originPos = Transform.Pos;

            length = details.Params[2];
            speed = (details.Params[1] > short.MaxValue ? -(ushort.MaxValue - details.Params[2]) : details.Params[2]) * 0.78f;
            ushort sync = details.Params[0];
            isSwing = details.Params[3] != 0;
            shade = details.Params[4] != 0;

            phase = (BaseCycleFrames - (float)(Time.GameTimer.TotalMilliseconds % BaseCycleFrames - sync * 175) * speed) % BaseCycleFrames;

            base.canBeFrozen = false;
            base.isInvulnerable = true;
            base.collisionFlags = CollisionFlags.CollideWithOtherActors;

            await RequestMetadataAsync("MovingPlatform/SpikeBall");
            SetAnimation("Platform");

            pieces = new ChainPiece[length];
            for (int i = 0; i < length; i++) {
                pieces[i] = new ChainPiece(api, originPos + new Vector3(0f, 0f, 4f), i);
                api.AddActor(pieces[i]);
            }
        }

        protected override void OnDeactivated(Component.ShutdownContext context)
        {
            for (int i = 0; i < length; i++) {
                api.RemoveActor(pieces[i]);
            }
        }

        protected override void OnUpdate()
        {
            phase -= speed * Time.TimeMult;
            if (phase < 0f) {
                phase += BaseCycleFrames;
            }

            float scale;
            Transform.Pos = GetPhasePosition(false, length, out scale);
            OnUpdateHitbox();

            canHurtPlayer = MathF.Abs(1f - scale) < 0.06f;

            Transform.Scale = scale;

            if (shade) {
                if (scale < 1f) {
                    renderer.ColorTint = new ColorRgba(scale, 1f);
                } else {
                    renderer.ColorTint = ColorRgba.White;
                }
            }

            for (int i = 0; i < length; i++) {
                pieces[i].Transform.Pos = GetPhasePosition(false, i, out scale);
                pieces[i].Transform.Scale = scale;

                // ToDo: Shade chain pieces
            }

            base.OnUpdate();
        }

        public Vector3 GetPhasePosition(bool next, int distance, out float scale)
        {
            if (length == 0) {
                scale = 1f;
                return originPos;
            }

            float effectivePhase = phase;
            if (next) {
                effectivePhase -= speed * Time.TimeMult;
            }

            if (isSwing) {
                // Mirror the upper half of the circle motion
                if (effectivePhase > BaseCycleFrames / 2) {
                    effectivePhase = BaseCycleFrames - effectivePhase;
                }

                // Blinn-Wyvill approximation to the raised inverted cosine,
                // easing curve with slower ends and faster middle part
                float i = (effectivePhase / BaseCycleFrames * 2);
                effectivePhase = (1 + ((4f / 9f) * MathF.Pow(i, 6) - (17f / 9f) * MathF.Pow(i, 4) + (22f / 9f) * MathF.Pow(i, 2))) * BaseCycleFrames / 2;
            }

            float multiX = MathF.Cos(effectivePhase / BaseCycleFrames * MathF.TwoPi);
            float multiY = MathF.Sin(effectivePhase / BaseCycleFrames * MathF.TwoPi);

            scale = 1f + multiX * 0.4f * distance / length;
            return new Vector3(
                originPos.X,
                originPos.Y - multiY * distance * 12f,
                originPos.Z - 10f - multiX * 10f * distance / length
            );
        }

        private class ChainPiece : ActorBase
        {
            private int distance;

            public ChainPiece(ActorApi api, Vector3 pos, int distance)
            {
                this.api = api;
                this.distance = distance;

                Transform transform = AddComponent<Transform>();
                Transform.Pos = pos;

                collisionFlags = CollisionFlags.ForceDisableCollisions;

                RequestMetadata("MovingPlatform/SpikeBall");
                SetAnimation("Chain");
            }

            protected override void OnUpdate()
            {
                // Controlled by SpikeBall
            }
        }
    }
}