using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Racetrack.GameServer.Models;
using WebGrease.Css.Extensions;

namespace Racetrack.GameServer.AI {
	public class Strategy {
		private const int MaxPathLength = int.MaxValue;
		private readonly WorldModel _worldModel;
		private readonly PlayerModel _curPlayer;
		private readonly List<List<int>> _distancesToFinish = new List<List<int>>();
		private readonly List<MoveModel> _optimalPath = new List<MoveModel>();
		private int _pathPosition;
		private int _maxLapLength;
		private readonly int _lapsCount;

		public Strategy(WorldModel world, PlayerModel curPlayer, int lapsCount) {
			_worldModel = world;
			_curPlayer = curPlayer;
			_lapsCount = lapsCount;

			CalculateDistancesToFinish();
			FindPath(_curPlayer);
		}

		public MoveModel GetNextStep() {
			return _optimalPath[_pathPosition++];
		}

		private void AddLine(Queue<PlayerModel> bfsQueue) {
			Line wayPoint = _worldModel.GetFinishLine();

			double dx = wayPoint.Second.X - wayPoint.First.X;
			double dy = wayPoint.Second.Y - wayPoint.First.Y;

			int minX = Math.Min(wayPoint.First.X, wayPoint.Second.X);
			int minY = Math.Min(wayPoint.First.Y, wayPoint.Second.Y);
			int maxX = Math.Max(wayPoint.First.X, wayPoint.Second.X);
			int maxY = Math.Max(wayPoint.First.Y, wayPoint.Second.Y);

			double deltaX = (dx > dy) ? 1 : (dx / dy);
			double deltaY = (dx > dy) ? (dy / dx) : 1;

			for (double i = wayPoint.First.X; i >= minX && i <= maxX; i += deltaX) {
				for (double j = wayPoint.First.Y; j >= minY && j <= maxY; j += deltaY) {
					for (int di = -1; di <= 1; ++di) {
						for (int dj = -1; dj <= 1; ++dj) {
							if (_worldModel.IsEmptyPosition((int) (i + di), (int) (j + dj))
									&& _worldModel.IsOnRightSideOfFinishLine((int) (i + di), (int) (j + dj))) {
								bfsQueue.Enqueue(new PlayerModel(
									new Coordinates((int) (i + di), (int) (j + dj)), _curPlayer.PlayerName));
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

		private int GetNextWayPoint(PlayerModel curState) {
			if (curState.LastWayPoint == -1) {
				return _worldModel.WayPointsCount() - 2;
			} else if (curState.LastWayPoint == 0) {
				return _worldModel.WayPointsCount() - 1;
			} else {
				return curState.LastWayPoint - 1;
			}
		}

		private void CalculateDistances(Queue<PlayerModel> bfsQueue) {
			while (bfsQueue.Count != 0) {
				var curNode = bfsQueue.Dequeue();

				if (_distancesToFinish[curNode.CurPosition.Y][curNode.CurPosition.X] != -1) {
					continue;
				}
				// Ищем минимальное значение расстояния до финиша среди соседей
				int minDistance = MaxPathLength;
				for (int i = -1; i <= 1; ++i) {
					for (int j = -1; j <= 1; ++j) {
						if (!_worldModel.IsEmptyPosition(curNode.CurPosition.X + j, curNode.CurPosition.Y + i)) {
							continue;
						}
						// Если на клетку можно встать, её еще не посетили 
						//  и она не находится с другой стороны от финиша - идем в неё
						var movementLine = new Line(new Coordinates(curNode.CurPosition.X, curNode.CurPosition.Y),
							new Coordinates(curNode.CurPosition.X + j, curNode.CurPosition.Y + i));

						var wayPoints = _worldModel.FindIntersectedWayPoints(movementLine);
						int nextWayPoint = GetNextWayPoint(curNode);

						if (_distancesToFinish[curNode.CurPosition.Y + i][curNode.CurPosition.X + j] == -1) {
							if (wayPoints.Count == 0 || wayPoints.Contains(curNode.LastWayPoint) || wayPoints.Contains(nextWayPoint)) {
								// Если пересекаем следующий вэйпоинт
								PlayerModel nextState = (PlayerModel) curNode.Clone();
								nextState.CurPosition.X += j;
								nextState.CurPosition.Y += i;
								if (wayPoints.Contains(nextWayPoint)) {
									nextState.LastWayPoint = nextWayPoint;
								}
								bfsQueue.Enqueue(nextState);
							}
						} else {
							// Уже посещенная или стартовая вершина
							if (curNode.LastWayPoint != 0 || !wayPoints.Contains(_worldModel.WayPointsCount() - 1)) {
								minDistance = Math.Min(_distancesToFinish[curNode.CurPosition.Y + i][curNode.CurPosition.X + j],
									minDistance);
							}
						}
					}
				}
				// Мин. расстояние до текущей вершины выражается через мин. расстояние до соседей
				_distancesToFinish[curNode.CurPosition.Y][curNode.CurPosition.X] = minDistance + 1;
			}
		}

		private void CalculateDistancesToFinish() {
			for (int i = 0; i < _worldModel.Height; ++i) {
				_distancesToFinish.Add(new List<int>(Enumerable.Repeat(-1, _worldModel.Width)));
			}

			var bfsQueue = new Queue<PlayerModel>();
			// Считаем дистанции при прохождении через промежуточные wayPoints
			AddLine(bfsQueue);
			CalculateDistances(bfsQueue);

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
			return _distancesToFinish[nextState.CurPosition.Y][nextState.CurPosition.X];
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
			openSet.Add(initState);
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

				AddNeighbors(curState, closedSet, gScore, cameFrom, fScore, openSet, openSetVal);
			}
			return false;
		}

		private void AddNeighbors(PlayerModel curState, HashSet<PlayerModel> closedSet, Dictionary<PlayerModel, int> gScore, 
				IDictionary<PlayerModel, PlayerModel> cameFrom, Dictionary<PlayerModel, int> fScore, 
				ISet<PlayerModel> openSet, IDictionary<int, List<PlayerModel>> openSetVal) {

			int nextWayPoint = GetNextWayPoint(curState);
			for (int i = -1; i <= 1; ++i) {
				for (int j = -1; j <= 1; ++j) {
					if (!_worldModel.IsEmptyPosition(curState.CurPosition.X + curState.Inertia.X + j,
							curState.CurPosition.Y + curState.Inertia.Y + i)) {
						continue;
					}

					var neighborState = curState.GetMovementResult(j, i);
					var wayPoints = _worldModel.FindIntersectedWayPoints(
						new Line(curState.CurPosition, neighborState.CurPosition));
					if (wayPoints.Count == 0 || wayPoints.Contains(curState.LastWayPoint) || wayPoints.Contains(nextWayPoint)) {
						if (wayPoints.Contains(nextWayPoint)) {
							neighborState.LastWayPoint = nextWayPoint;
						}
					} else {
						continue;
					}

//					// Запрещаем пересекать финиш раньше времени
//					if (_worldModel.IsFinishLineIntersected(new Line(curState.CurPosition, neighborState.CurPosition))
//							&& curState.LastWayPoint != _worldModel.WayPointsCount() - 2) {
//						continue;
//					}

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

		private void ReconstructPath(Dictionary<PlayerModel, PlayerModel> cameFrom, PlayerModel goal) {
			var curState = goal;
			_optimalPath.Clear();

			for (; cameFrom[curState] != curState;) {
				var next = cameFrom[curState];

				var prevInertia = next.Inertia;
				_optimalPath.Add(new MoveModel(curState.Inertia.X - prevInertia.X, curState.Inertia.Y - prevInertia.Y));

				curState = next;
			}

			_optimalPath.Reverse();
		}
	}
}