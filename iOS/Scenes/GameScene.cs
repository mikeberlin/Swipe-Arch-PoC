using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.SpriteKit;
using MonoTouch.UIKit;
using System.Linq;

namespace iOS
{
	public class GameScene : SKScene
	{
		#region Constants and Properties

		private const string SPRITE_NAME = "Sprite-To-Swipe";

		private SKSpriteNode Background { get; set; }
		private SKSpriteNode Ground { get; set; }
		private SKSpriteNode SelectedNode { get; set; }

		private PointF StartSwipePosition { get; set; }
		private DateTime StartSwipeTime { get; set; }

		#endregion

		#region Constructors

		public GameScene (SizeF size)
		{
			// Load the background
			this.Background = new SKSpriteNode ("blue-shooting-stars-vertical");
			this.Background.Name = "background";
			this.Background.AnchorPoint = new PointF (0, 0);
			this.AddChild (Background);

			this.Size = size;
			this.PhysicsWorld.Gravity = new CGVector (this.PhysicsWorld.Gravity.dx, -3.8f);
			this.PhysicsBody = SKPhysicsBody.BodyWithEdgeLoopFromRect (this.Background.Frame);

			// Setup the ground node
			this.Ground = new SKSpriteNode (UIColor.Brown, new SizeF (this.Frame.Width, 20f));
			this.Ground.Position = new PointF (this.Frame.Width / 2, 0);
			this.Ground.PhysicsBody = SKPhysicsBody.BodyWithRectangleOfSize (this.Ground.Size);
			this.Ground.PhysicsBody.Dynamic = false;
			this.Ground.PhysicsBody.AffectedByGravity = false;
			this.Background.AddChild (this.Ground);

			SKSpriteNode sprite = new SKSpriteNode ("dog");
			sprite.Name = SPRITE_NAME;
			sprite.Position = new PointF (35f, 50f);
			sprite.PhysicsBody = SKPhysicsBody.BodyWithRectangleOfSize (sprite.Size);
			sprite.PhysicsBody.Dynamic = true;
			sprite.PhysicsBody.AffectedByGravity = true;
			sprite.PhysicsBody.AllowsRotation = true;
			sprite.PhysicsBody.Mass = 0.4f;
			this.Background.AddChild (sprite);

			SKSpriteNode applyLeftImpulse = new SKSpriteNode (UIColor.Orange, new SizeF (50f, 50f));
			applyLeftImpulse.Position = new PointF (50f, 250f);
			applyLeftImpulse.Name = "APPLY-LEFT-IMPULSE";
			this.AddChild (applyLeftImpulse);

			SKSpriteNode applyRightImpulse = new SKSpriteNode (UIColor.Orange, new SizeF (50f, 50f));
			applyRightImpulse.Position = new PointF (270f, 250f);
			applyRightImpulse.Name = "APPLY-RIGHT-IMPULSE";
			this.AddChild (applyRightImpulse);
		}

		#endregion

		#region SKScene Events

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView (view);

			UIPanGestureRecognizer panGesture = new UIPanGestureRecognizer (HandlePanFrom);
			UITapGestureRecognizer tapGesture = new UITapGestureRecognizer (HandleTapFrom);

			this.View.AddGestureRecognizer (panGesture);
			this.View.AddGestureRecognizer (tapGesture);
		}

		public override void Update (double currentTime)
		{
			base.Update (currentTime);

			if ((this.SelectedNode != null) && (this.SelectedNode.Name == SPRITE_NAME)) {
				if (this.SelectedNode.Position.Y >= 500) {
					this.Background.Position = BoundLayerPosition (new PointF (this.Background.Position.X, this.Background.Position.Y - 80));
				} else if (this.SelectedNode.Position.Y + this.Background.Position.Y <= 300) {
					this.Background.Position = BoundLayerPosition (new PointF (this.Background.Position.X, this.Background.Position.Y + 80));
				}
			}
		}

		#endregion

		#region Private Methods

		private void HandlePanFrom(UIPanGestureRecognizer recognizer)
		{
			if (recognizer.State == UIGestureRecognizerState.Began) {
				PointF currentLocation = recognizer.LocationInView (recognizer.View);
				currentLocation = this.ConvertPointFromView (currentLocation);

				this.SelectedNode = (SKSpriteNode)this.GetNodeAtPoint (currentLocation);
			}
			else if (recognizer.State == UIGestureRecognizerState.Changed || this.SelectedNode.Name != SPRITE_NAME) {
				PointF translation = recognizer.TranslationInView (recognizer.View);
				translation = new PointF (translation.X, -translation.Y);

				this.Background.Position = this.BoundLayerPosition(new PointF (this.SelectedNode.Position.X + translation.X, this.SelectedNode.Position.Y + translation.Y));
			}
			else if (recognizer.State == UIGestureRecognizerState.Ended) {
				if (this.SelectedNode.Name == SPRITE_NAME) {
					PointF velocity = recognizer.VelocityInView (recognizer.View);
					velocity = new PointF (velocity.X, -velocity.Y);

					this.SelectedNode.PhysicsBody.ApplyImpulse (new CGVector (velocity.X, velocity.Y));
				}
			}
		}

		private void HandleTapFrom(UITapGestureRecognizer recognizer)
		{
			PointF currentLocation = recognizer.LocationInView (recognizer.View);
			currentLocation = this.ConvertPointFromView (currentLocation);

			SKSpriteNode tappedNode = (SKSpriteNode)this.GetNodeAtPoint (currentLocation);

			if (tappedNode.Name == "APPLY-LEFT-IMPULSE") {
				this.SelectedNode.PhysicsBody.ApplyImpulse (new CGVector (-25f, 0));
			} else if (tappedNode.Name == "APPLY-RIGHT-IMPULSE") {
				this.SelectedNode.PhysicsBody.ApplyImpulse (new CGVector (25f, 0));
			}
		}

		private PointF BoundLayerPosition(PointF boundedPoint)
		{
			PointF returnValue = boundedPoint;
			returnValue.X = this.Position.X;
			returnValue.Y = Math.Min (returnValue.Y, 0);
			returnValue.Y = Math.Max (returnValue.Y, this.Size.Height - this.Background.Size.Height);

			return returnValue;
		}

		#endregion
	}
}