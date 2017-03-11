using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Racetrack.Models;

namespace Racetrack.Context {
	public class ResultContext : DbContext {
		public ResultContext() : base("ResultContext") 
		{}

		public DbSet<Result> Results { get; set; }
	}
}