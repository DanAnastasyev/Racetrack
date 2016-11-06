using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Racetrack.GameServer.Models;
using WebGrease.Css.Extensions;

namespace Racetrack.GameServer.AI {
	public class Strategy {
		private static readonly int MaxPathLength = int.MaxValue;
		private readonly WorldModel _worldModel;
		private readonly PlayerModel _curPlayer;
		private readonly List<List<int>> _distancesToFinish = new List<List<int>>();
		private readonly List<MoveModel> _optimalPath = new List<MoveModel>();
		private int _maxLapLength;
		private readonly int _lapsCount;

		public Strategy(WorldModel world, PlayerModel curPlayer, int lapsCount) {
			_worldModel = world;
			_curPlayer = curPlayer;
			_lapsCount = lapsCount;

			CalculateDistancesToFinish();
			FindPath(_curPlayer);
		}

		private void AddFinishLine(Queue<Coordinates> bfsQueue) {
			Line finishLine = _worldModel.GetFinishLine();

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
							if (_worldModel.IsEmptyPosition((int) (i + di), (int) (j + dj))
									&& _worldModel.IsOnRightSideOfFinishLine((int) (i + di), (int) (j + dj))) {
								bfsQueue.Enqueue(new Coordinates((int) (i + di), (int) (j + dj)));
							}
						}
					}
					_distancesToFinish[(int) j][(int) i] = 0;

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
			_distancesToFinish.AddRange(Enumerable.Repeat(new List<int>(), _worldModel.Height));
			_distancesToFinish.ForEach(list => list.AddRange(Enumerable.Repeat(-1, _worldModel.Width)));

			var bfsQueue = new Queue<Coordinates>();
			AddFinishLine(bfsQueue);
			var finishLineNumber = _worldModel.WayPointsCount() - 1;

			while (bfsQueue.Count != 0) {
				var curNode = bfsQueue.Dequeue();

				if (_distancesToFinish[curNode.Y][curNode.X] != -1) {
					continue;
				}
				// Ищем минимальное значение расстояния до финиша среди соседей
				int minDistance = MaxPathLength; 
				for (int i = -1; i <= 1; ++i) {
					for (int j = -1; j <= 1; ++j) {
						if (!_worldModel.IsEmptyPosition(curNode.X + j, curNode.Y + i)) {
							continue;
						}
						// Если на клетку можно встать, её еще не посетили 
						//  и она не находится с другой стороны от финиша - идем в неё
						var movementLine = new Line(new Coordinates(curNode.Y, curNode.X),
							new Coordinates(curNode.Y + j, curNode.X + i));
						var reverseMovementLine = new Line(new Coordinates(curNode.Y + j, curNode.X + i),
							new Coordinates(curNode.Y, curNode.X));
						if (!_worldModel.FindIntersectedWayPoints(movementLine).Contains(finishLineNumber)
								&& _distancesToFinish[curNode.Y + j][curNode.X + i] == -1) {
							bfsQueue.Enqueue(new Coordinates(curNode.X + j, curNode.Y + i));
						} else if (!_worldModel.FindIntersectedWayPoints(reverseMovementLine).Contains(finishLineNumber)
								&& _distancesToFinish[curNode.Y + j][curNode.X + i] != -1) {
							// Уже посещенная вершина
							minDistance = Math.Min(_distancesToFinish[curNode.Y + j][curNode.X + i], minDistance);
						}
					}
				}
				// Мин. расстояние до текущей вершины выражается через мин. расстояние до соседей
				_distancesToFinish[curNode.Y][curNode.X] = minDistance + 1;
			}

			foreach (var distances in _distancesToFinish) {
				foreach (var distance in distances) {
					_maxLapLength = Math.Max(distance, _maxLapLength);
				}
			}
		}

		private int CalculateHeuristic(PlayerModel state, PlayerModel nextState) {
			if (_worldModel.IsMovementOutOfTrack(new Line(state.CurPosition, nextState.CurPosition))) {
				return int.MaxValue;
			}
			if (_distancesToFinish[nextState.CurPosition.Y][nextState.CurPosition.X] == -1) {
				return int.MaxValue;
			}
			return ( _distancesToFinish[nextState.CurPosition.Y][nextState.CurPosition.X]
				+ (_lapsCount - state.CurLap) * _maxLapLength ) / 4;
		}

		private bool FindPath(PlayerModel initState) {
			var closedSet = new HashSet<PlayerModel>();
			var openSetVal = new SortedDictionary<int, List<PlayerModel>>();
			var openSet = new HashSet<PlayerModel>();
			var cameFrom = new Dictionary<PlayerModel, PlayerModel>();
			var gScore = new Dictionary<PlayerModel, int>();
			var fScore = new Dictionary<PlayerModel, int>();

			if (!gScore.ContainsKey(initState)) {
				gScore[initState] = 0;
				fScore[initState] = gScore[initState] + CalculateHeuristic(initState, initState);
			}
			if (!openSetVal.ContainsKey(fScore[initState])) {
				openSetVal[fScore[initState]] = new List<PlayerModel>();
			}
			openSetVal[fScore[initState]].Add(initState);
			cameFrom[initState] = initState;

			while (openSetVal.Count != 0) {
				PlayerModel curState = openSetVal.First().Value.First();
				if (!openSet.Contains(curState)) {
					// Если это уже пройденное состояние
					continue;
				}
				openSet.Remove(curState);
				openSetVal.First().Value.Remove(curState);
				if (openSetVal.First().Value.Count == 0) {
					openSetVal.Remove(openSetVal.First().Key);
				}
				closedSet.Add(curState);

				if (_worldModel.IsFinishLineIntersected(new Line(cameFrom[curState].CurPosition, curState.CurPosition))
						&& curState.LastWayPoint == _worldModel.WayPointsCount() - 1) {
					++curState.CurLap;
					if (curState.CurLap == _lapsCount) {
						ReconstructPath(cameFrom, curState);
						return true;
					}
				}

				for (int i = -1; i <= 1; ++i) {
					for (int j = -1; j <= 1; ++j) {
						if (!_worldModel.IsEmptyPosition(curState.CurPosition.X + curState.Inertia.X + j,
								curState.CurPosition.Y + curState.Inertia.Y + i)) {
							continue;
						}

						var neighborState = curState.GetMovementResult(j, i);

						if (closedSet.Contains(neighborState)) {
							continue;
						}

						int tentGScore = gScore[curState] + 1;

						if (gScore.ContainsKey(neighborState) && (tentGScore > gScore[neighborState])) {
							continue;
						}
						cameFrom[neighborState] = curState;
						gScore[neighborState] = tentGScore;
						int heuristic = CalculateHeuristic(curState, neighborState);
						if (heuristic == int.MaxValue) {
							continue;
						}
						fScore[neighborState] = gScore[neighborState] + heuristic;
						if (!openSet.Add(neighborState)) {
							continue;
						}
						if (!openSetVal.ContainsKey(fScore[neighborState])) {
							openSetVal[fScore[neighborState]] = new List<PlayerModel>();
						}
						openSetVal[fScore[neighborState]].Add(neighborState);
					}
				}
			}
			return false;
		}

		private void ReconstructPath(Dictionary<PlayerModel, PlayerModel> cameFrom, PlayerModel goal) {
			var curState = goal;
			_optimalPath.Clear();

			for (; cameFrom[curState] != curState;) {
				var next = cameFrom[curState];

				var prevPosition = (Coordinates) next.PrevPosition.Clone();
				var prevVelocity = next.Inertia;
				prevPosition.X += prevVelocity.X;
				prevPosition.Y += prevVelocity.Y;
				_optimalPath.Add(new MoveModel(curState.CurPosition.X - prevPosition.X, curState.CurPosition.Y - prevPosition.Y));

				curState = next;
			}

			_optimalPath.Reverse();
		}
	}
}