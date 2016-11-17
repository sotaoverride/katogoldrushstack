using System;
using System.Collections.Generic;
using CocosSharp;

namespace BouncingGame
{
	class Stack
	{
		Object[] stack;
		Int32 i;
		Int32 j;

		public Stack(int n)
		{
			stack = new Object[n];
			i = 0;
			j = n;
		}

		public void Push(object item)
		{
			if (!isStackFull())
			{
				stack[i++] = item;
			}

		}

		public bool isStackFull()
		{
			if (i == j)
				return true;
			else
				return false;
		}

		public object Pop()
		{
			if (stack.Length != 0)
				return stack[--i];
			return -1;
			//Console.WriteLine("Stack is empty");
		}

		public object TopElement()
		{
			if (stack.Length != 0)
				return stack[i - 1];
			return 0;
		}
	}

	public class GameLayer : CCLayerColor
	{
		CCSprite paddleSprite;
		CCSprite ballSprite;
		CCLabel scoreLabel;
		CCSprite trackSeg1;
		CCSprite trackSeg2;
		CCSprite trackSeg3;

		float ballXVelocity;
		float ballYVelocity;
		float trackSegYVelocity;

		// How much to modify the ball's y velocity per second:
		const float gravity = 140;

		int score;

		//Aatir:
		//Stack for trackSegments
		Stack trackSegmentStack = new Stack(3);




		public GameLayer () : base (CCColor4B.Black)
		{
			//Aatir:
			// push the three track segments onto stack
			trackSegmentStack.Push(new CCSprite("track_05"));
			trackSegmentStack.Push(new CCSprite("track_05"));
			trackSegmentStack.Push(new CCSprite("track_05"));
			//start poping the track segments 
			trackSeg1 = (CocosSharp.CCSprite)trackSegmentStack.Pop();
			trackSeg2 = (CocosSharp.CCSprite)trackSegmentStack.Pop();
			trackSeg3 = (CocosSharp.CCSprite)trackSegmentStack.Pop();

			trackSeg1.PositionX = 344;
			trackSeg1.PositionY = 1000;
			AddChild(trackSeg1);
			//check this
			trackSeg2.PositionX = 384;
			trackSeg2.PositionY = 1000;
			AddChild(trackSeg2);

			trackSeg3.PositionX = 424;
			trackSeg3.PositionY = 1000;
			AddChild(trackSeg3);

			//int width = 768;
			//int height = 1027;

			ballSprite = new CCSprite ("train_01");
			ballSprite.PositionX = 384;
			ballSprite.PositionY = 20;
			AddChild (ballSprite);

			scoreLabel = new CCLabel ("Score: 0", "Arial", 20, CCLabelFormat.SystemFont);
			scoreLabel.PositionX = 50;
			scoreLabel.PositionY = 1000;
			scoreLabel.AnchorPoint = CCPoint.AnchorUpperLeft;
			AddChild (scoreLabel);

			Schedule (RunGameLogic);
		}

		void RunGameLogic(float frameTimeInSeconds)
		{

			//Aatir:
			//Make a new track appear from the array
			//AddChild (trackSprite[i])
			//trackMaker = new CCSprite("train_01");

			// This is a linear approximation, so not 100% accurate
			//Aatir:
			//Change velocity to zero
			//ballYVelocity += frameTimeInSeconds * -gravity;
			ballYVelocity = 0;
			trackSegYVelocity += frameTimeInSeconds * -gravity;
			trackSeg1.PositionY += trackSegYVelocity * frameTimeInSeconds;
			trackSeg2.PositionY += trackSegYVelocity * frameTimeInSeconds;
			trackSeg3.PositionY += trackSegYVelocity * frameTimeInSeconds;
			if (trackSeg3.PositionY* trackSeg1.PositionY *trackSeg2.PositionY < 0)
			{
				trackSeg1.PositionY = 1000;
				trackSeg2.PositionY = 1000;
				trackSeg3.PositionY = 1000;
			}
			//ballSprite.PositionX += ballXVelocity * frameTimeInSeconds;
			//ballSprite.PositionY += ballYVelocity * frameTimeInSeconds;
			// New Code:
			// Check if the two CCSprites overlap...
			//Aatir
			//Make this for Train  engine to overlap track
			//bool doesBallOverlapPaddle = ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
			//	paddleSprite.BoundingBoxTransformedToParent);
			bool doesTrackOverlapTrain =( ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
				trackSeg1.BoundingBoxTransformedToParent) || ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
				trackSeg2.BoundingBoxTransformedToParent) || ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
				trackSeg3.BoundingBoxTransformedToParent) );
			// ... and if the ball is moving downward.
			bool isMovingDownward = ballYVelocity < 0;
			/*	if (doesBallOverlapPaddle && isMovingDownward)
				{
					// First let's invert the velocity:
					ballYVelocity *= -1;
					// Then let's assign a random to the ball's x velocity:
					const float minXVelocity = -300;
					const float maxXVelocity = 300;
					ballXVelocity = CCRandom.GetRandomFloat (minXVelocity, maxXVelocity);
					// New code:
					score++;
					scoreLabel.Text = "Score: " + score;
				}*/

			if (doesTrackOverlapTrain ){
				
				trackSeg1.PositionY = 1000;
				trackSeg2.PositionY = 1000;
				trackSeg3.PositionY = 1000;
				score++;
				scoreLabel.Text = "Score: " + score;
				
			}
			// First let’s get the ball position:   
			float ballRight = ballSprite.BoundingBoxTransformedToParent.MaxX;
			float ballLeft = ballSprite.BoundingBoxTransformedToParent.MinX;
			// Then let’s get the screen edges
			float screenRight = VisibleBoundsWorldspace.MaxX;
			float screenLeft = VisibleBoundsWorldspace.MinX;

			// Check if the ball is either too far to the right or left:    
			bool shouldReflectXVelocity = 
				(ballRight > screenRight && ballXVelocity > 0) ||
				(ballLeft < screenLeft && ballXVelocity < 0);

			if (shouldReflectXVelocity)
			{
				ballXVelocity *= -1;
			}
		}

		protected override void AddedToScene ()
		{
			base.AddedToScene ();

			// Use the bounds to layout the positioning of our drawable assets
			CCRect bounds = VisibleBoundsWorldspace;

			// Register for touch events
			var touchListener = new CCEventListenerTouchAllAtOnce ();
			touchListener.OnTouchesEnded = OnTouchesEnded;
			touchListener.OnTouchesMoved = HandleTouchesMoved;
			AddEventListener (touchListener, this);
		}

		void OnTouchesEnded (List<CCTouch> touches, CCEvent touchEvent)
		{
			if (touches.Count > 0)
			{
				// Perform touch handling here
			}
		}

		void HandleTouchesMoved (System.Collections.Generic.List<CCTouch> touches, CCEvent touchEvent)
		{
			// we only care about the first touch:

			//Aatir
			//for track add a for loop here and do it for all of them
			float tmpDis = trackSeg3.PositionX - trackSeg2.PositionX;
			var locationOnScreen = touches [0].Location;
			trackSeg1.PositionX = locationOnScreen.X-40;
			trackSeg2.PositionX = locationOnScreen.X ;
			trackSeg3.PositionX = locationOnScreen.X +40;

		}
	}
}
