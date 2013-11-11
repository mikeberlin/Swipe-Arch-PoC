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
		private SKSpriteNode SelectedNode { get; set; }

		#endregion

		#region Constructors

		public GameScene (SizeF size)
		{
			this.Size = size;

			// Load the background
			this.Background = new SKSpriteNode ("blue-shooting-stars-vertical");
			this.Background.Name = "background";
			this.Background.AnchorPoint = new PointF (0, 0);
			this.AddChild (Background);

			// Loading the images and adding to scene
			List<string> imageNames = new List<string>() {"bird", "cat", "dog", "turtle"};

			int i = 0;
			foreach (string imageName in imageNames) {
				float offsetFraction = ((float)(i++ + 1) / (imageNames.Count + 1));

				SKSpriteNode sprite = new SKSpriteNode (imageName);
				sprite.Name = SPRITE_NAME;
				sprite.Position = new PointF (size.Width * offsetFraction, 50f);

				this.Background.AddChild (sprite);
			}
		}

		#endregion

		#region SKScene Events

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView (view);

			UIPanGestureRecognizer gestureRecognizer = new UIPanGestureRecognizer (HandlePanFrom);
			this.View.AddGestureRecognizer (gestureRecognizer);
		}

		public override void Update (double currentTime)
		{
			base.Update (currentTime);

			if ((this.SelectedNode != null) && (this.SelectedNode.Name == SPRITE_NAME)) {
				if (this.SelectedNode.Position.Y >= 300) {
					this.Background.Position = BoundLayerPosition (new PointF (this.Background.Position.X, this.Background.Position.Y - 20));
				} else if (this.SelectedNode.Position.Y + this.Background.Position.Y <= 100) {
					this.Background.Position = BoundLayerPosition (new PointF (this.Background.Position.X, this.Background.Position.Y + 20));
				}
			}
		}

		#endregion

		#region Private Methods

		private void PanForTranslation(PointF translation)
		{
			PointF translatedPoint = new PointF (this.SelectedNode.Position.X + translation.X,
			                                     this.SelectedNode.Position.Y + translation.Y);

			if (this.SelectedNode.Name == SPRITE_NAME) {
				this.SelectedNode.Position = translatedPoint;
			}
			else {
				PointF boundedPoint = translatedPoint;
				boundedPoint.X = this.Position.X;
				boundedPoint.Y = Math.Max (Math.Min (boundedPoint.Y, 0), (this.Size.Height - this.Background.Size.Height));
				this.Background.Position = boundedPoint;
			}
		}

		private PointF BoundLayerPosition(PointF boundedPoint)
		{
			SizeF windowSize = this.Size;

			PointF returnValue = boundedPoint;
			returnValue.X = this.Position.X;
			returnValue.Y = Math.Min (returnValue.Y, 0);
			returnValue.Y = Math.Max (returnValue.Y, windowSize.Height - this.Background.Size.Height);

			return returnValue;
		}

		private void HandlePanFrom(UIPanGestureRecognizer recognizer)
		{
			PointF touchLocation = recognizer.LocationInView (recognizer.View);
			touchLocation = this.ConvertPointFromView (touchLocation);

			SKSpriteNode touchedNode = (SKSpriteNode)this.GetNodeAtPoint (touchLocation);

			PointF translation = recognizer.TranslationInView (this.View);
			translation = new PointF (translation.X, -translation.Y);

			if (recognizer.State == UIGestureRecognizerState.Began) {
				this.SelectedNode = touchedNode;
			}
			else if (recognizer.State == UIGestureRecognizerState.Changed) {
				this.PanForTranslation (translation);
				recognizer.SetTranslation (new PointF (0, 0), this.View);
			}
			else if (recognizer.State == UIGestureRecognizerState.Ended) {
				if (this.SelectedNode.Name == SPRITE_NAME) {
					PointF velocity = recognizer.VelocityInView (this.View);
					velocity = new PointF (velocity.X, -velocity.Y);
					float magnitude = (float)Math.Sqrt ((velocity.X * velocity.X) + (velocity.Y * velocity.Y));
					float slideMult = (magnitude / 300);

					// increase for more of a slide
					float slideFactor = (float)(0.5 * slideMult);

					PointF finalPoint = new PointF (this.SelectedNode.Position.X + (velocity.X * slideFactor),
					                               this.SelectedNode.Position.Y + (velocity.Y * slideFactor));

					finalPoint.X = Math.Min (Math.Max (finalPoint.X, 0), this.Background.Size.Width);
					finalPoint.Y = Math.Min (Math.Max (finalPoint.Y, 0), this.Background.Size.Height);

					this.SelectedNode.RemoveAllActions ();

					// hoping that I can use the physics engine to assist with the arch
					this.SelectedNode.PhysicsBody = SKPhysicsBody.BodyWithRectangleOfSize(this.SelectedNode.Size);
					this.SelectedNode.PhysicsBody.Dynamic = true;

					SKAction moveTo = SKAction.MoveTo (finalPoint, slideFactor);
					moveTo.TimingMode = SKActionTimingMode.EaseOut;
					this.SelectedNode.RunAction (moveTo, () => {
						// TODO: Callback, but need to start curving at top of arch before this happens right?
					});
				}
			}
		}

		#endregion
	}
}