using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer.AI {
	public class Strategy {
		public readonly WorldModel WorldModel;
		private List<List<int>> distancesToFinish = new List<List<int>>();

		private void AddFinishLine(Queue<Coordinates> bfsQueue) {
			Line finishLine = WorldModel.GetFinishLine();

			double dx = finishLine.First.X - finishLine.Second.X;
			double dy = finishLine.First.Y - finishLine.Second.Y;

			int minX = Math.Min(finishLine.First.X, finishLine.Second.X);
			int minY = Math.Min(finishLine.First.Y, finishLine.Second.Y);
			int maxX = Math.Max(finishLine.First.X, finishLine.Second.X);
			int maxY = Math.Max(finishLine.First.Y, finishLine.Second.Y);

			double deltaX = (dx > dy) ? 1 : (dx / dy);
			double deltaY = (dx > dy) ? (dy / dx) : 1;

			for (double i = finishLine.First.X; i >= minX && i <= maxX; i += deltaX) {
				for (double j = finishLine.First.Y; j >= minY && j <= maxY; j += deltaY) {
					for (int di = -1; di <= 1; ++di) {
						for (int dj = -1; dj <= 1; ++dj) {
							if (WorldModel.IsEmptyPosition((int) (i + di), (int) (j + dj))
									&& WorldModel.IsOnRightSideOfFinishLine((int) (i + di), (int) (j + dj))) {
								bfsQueue.Enqueue(new Coordinates((int) (i + di), (int) (j + dj)));
							}
						}
					}
					distancesToFinish[(int) j][(int) i] = 0;

					if (Math.Abs(deltaY) < 0.0001) {
						break;
					}
				}
				if (Math.Abs(deltaX) < 0.0001) {
					break;
				}
			}
		}

		private void CalculateDistancesToFinish() {
			distancesToFinish.AddRange(Enumerable.Repeat(new List<int>(), WorldModel.Height));
			distancesToFinish.ForEach(list => list.AddRange(Enumerable.Repeat(-1, WorldModel.Width)));

			var bfsQueue = new Queue<Coordinates>();
			AddFinishLine(bfsQueue);


		}

//		public MoveModel NextMove(List<PlayerModel> players, int playerNumber) {
//			
//		}
	}
}