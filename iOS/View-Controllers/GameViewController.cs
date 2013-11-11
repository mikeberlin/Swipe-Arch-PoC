using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.SpriteKit;
using MonoTouch.UIKit;

namespace iOS
{
	public partial class GameViewController : UIViewController
	{
		public GameViewController () : base ()
		{
		}

		public override void LoadView ()
		{
			base.LoadView ();

			this.View = new SKView {
				ShowsFPS = true,
				ShowsNodeCount = true,
				ShowsDrawCount = true
			};
		}

		public override void ViewWillLayoutSubviews ()
		{
			base.ViewWillLayoutSubviews ();

			var view = (SKView)View;

			if (view.Scene == null) {
				var gameScene = new GameScene (this.View.Bounds.Size);
				gameScene.ScaleMode = SKSceneScaleMode.AspectFill;
				view.PresentScene (gameScene);
			}
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();

			// Release any cached data, images, etc that aren't in use.
		}
	}
}