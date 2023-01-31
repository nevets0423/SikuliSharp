using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SikuliSharp {
	public interface ISikuliSessionV2 : IDisposable {
		Match Find(IPattern pattern);
		List<Match> FindAll(IPattern pattern);
		Match Wait(IPattern pattern, float timeoutInSeconds = 0);
		bool WaitVanish(IPattern pattern, float timeoutInSeconds = 0);
		Match Exists(IPattern pattern, float timeoutInSeconds = 0);
		bool Has(IPattern pattern, float timeoutInSeconds = 0);
		Match FindBest(params IPattern[] pattern);
		Match WaitBest(float timeoutInSeconds = 0, params IPattern[] pattern);
		Match FindAny(params IPattern[] patterns);
		Match WaitAny(float timeoutInSeconds = 0, params IPattern[] patterns);
		bool Click(IPattern pattern);
		bool Click(IPattern pattern, Point offset);
		bool Click(IRegion region);
		bool DoubleClick(IPattern pattern);
		bool DoubleClick(IPattern pattern, Point offset);
		bool DoubleClick(IRegion region);
		bool Hover(IPattern pattern);
		bool Hover(IPattern pattern, Point offset);
		bool Hover(IRegion region);
		bool RightClick(IPattern pattern);
		bool RightClick(IPattern pattern, Point offset);
		bool RightClick(IRegion region);
		bool DragDrop(IPattern fromPattern, IPattern toPattern);
		bool DragDrop(IRegion fromRegion, IRegion toRegion);
		bool Highlight(IRegion region);
		bool Highlight(IRegion region, string color);
		bool Highlight(IRegion region, double seconds);
		bool Highlight(IRegion region, double seconds, string color);
	}

	public class SikuliSessionV2 : ISikuliSessionV2 {

		private static readonly Regex MatchTextRegex = new Regex(KeyWords.MatchTextRegex, RegexOptions.Compiled);
		private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
		private readonly ISikuliRuntime _runtime;

		public SikuliSessionV2(ISikuliRuntime sikuliRuntime) {
			_runtime = sikuliRuntime;
			_runtime.Start();
		}

		#region Actions

		public Match Find(IPattern pattern) {
			var commandResult = RunCommandWithReturn("find", pattern, 0);
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public List<Match> FindAll(IPattern pattern) {
			var commandResult = RunCommandWithMultiReturn("findAll", pattern, 0);
			var results = new List<Match>();
			foreach (System.Text.RegularExpressions.Match m in SikuliSessionV2.MatchTextRegex.Matches(commandResult)) {
				Console.WriteLine(m);
				results.Add(new Match(Convert.ToString(m)));
			}
			return results;
		}

		public Match Wait(IPattern pattern, float timeoutInSeconds = 0) {
			var commandResult = RunCommandWithReturn("wait", pattern, timeoutInSeconds);
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public bool WaitVanish(IPattern pattern, float timeoutInSeconds = 0) {
			return RunCommand("waitVanish", pattern, timeoutInSeconds);
		}

		public Match Exists(IPattern pattern, float timeoutInSeconds = 0) {
			var commandResult = RunCommandWithReturn("exists", pattern, timeoutInSeconds);
			if (commandResult.Contains(KeyWords.None)) {
				return null;
			}
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public bool Has(IPattern pattern, float timeoutInSeconds = 0) {
			return RunCommand("has", pattern, timeoutInSeconds);
		}

		public Match FindBest(params IPattern[] patterns) {
			var commandResult = RunCommandWithMultiReturn("findBest", patterns);
			if (commandResult.Contains(KeyWords.None)) {
				return null;
			}
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public Match WaitBest(float timeoutInSeconds = 0, params IPattern[] patterns) {
			var commandResult = RunCommandWithMultiReturn("waitBest", timeoutInSeconds, patterns);
			if (commandResult.Contains(KeyWords.None)) {
				return null;
			}
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public Match FindAny(params IPattern[] patterns) {
			var commandResult = RunCommandWithMultiReturn("findAny", patterns);
			if (commandResult.Contains(KeyWords.None)) {
				return null;
			}
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		public Match WaitAny(float timeoutInSeconds = 0, params IPattern[] patterns) {
			var commandResult = RunCommandWithMultiReturn("waitAny", timeoutInSeconds, patterns);
			if (commandResult.Contains(KeyWords.None)) {
				return null;
			}
			return new Match(Convert.ToString(SikuliSessionV2.MatchTextRegex.Match(commandResult)));
		}

		#endregion

		#region Mouse Actions (IPattern)
		public bool Click(IPattern pattern) {
			return RunCommand("click", pattern, 0);
		}

		public bool Click(IPattern pattern, Point offset) {
			return RunCommand("click", new WithOffsetPattern(pattern, offset), 0);
		}

		public bool DoubleClick(IPattern pattern) {
			return RunCommand("doubleClick", pattern, 0);
		}

		public bool DoubleClick(IPattern pattern, Point offset) {
			return RunCommand("doubleClick", new WithOffsetPattern(pattern, offset), 0);
		}

		public bool Hover(IPattern pattern) {
			return RunCommand("hover", pattern, 0);
		}

		public bool Hover(IPattern pattern, Point offset) {
			return RunCommand("hover", new WithOffsetPattern(pattern, offset), 0);
		}

		public bool RightClick(IPattern pattern) {
			return RunCommand("rightClick", pattern, 0);
		}

		public bool RightClick(IPattern pattern, Point offset) {
			return RunCommand("rightClick", new WithOffsetPattern(pattern, offset), 0);
		}

		public bool DragDrop(IPattern fromPattern, IPattern toPattern) {
			return RunCommand("dragDrop", fromPattern, toPattern, 0);
		}
		#endregion

		#region Mouse Actions (IRegion)
		public bool Click(IRegion region) {
			return RunCommand("click", region, 0);
		}

		public bool DoubleClick(IRegion region) {
			return RunCommand("doubleClick", region, 0);
		}

		public bool Hover(IRegion region) {
			return RunCommand("hover", region, 0);
		}

		public bool RightClick(IRegion region) {
			return RunCommand("rightClick", region, 0);
		}

		public bool DragDrop(IRegion fromRegion, IRegion toRegion) {
			return RunCommand("dragDrop", fromRegion, toRegion, 0);
		}
		#endregion

		#region Highlight
		public bool Highlight(IRegion region) {
			return RunDotCommand("highlight", region, null, 0);
		}

		public bool Highlight(IRegion region, string color) {
			string paramString = String.Format("'{0}'", color);
			return RunDotCommand("highlight", region, paramString, 0);
		}

		public bool Highlight(IRegion region, double seconds) {
			string paramString = seconds.ToString("0.####", InvariantCulture);
			return RunDotCommand("highlight", region, paramString, 0);
		}

		public bool Highlight(IRegion region, double seconds, string color) {
			string paramString = String.Format("{0}, '{1}'", seconds.ToString("0.####", InvariantCulture), color);
			return RunDotCommand("highlight", region, paramString, 0);
		}
		#endregion

		#region Runners
		#region IPattern
		protected bool RunCommand(string command, IPattern pattern, float commandParameter) {
			pattern.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} YES\" if {command}({pattern.ToSikuliScript()}{ToSukuliFloat(commandParameter)}) else \"{KeyWords.ReturnIdentifier} NO\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			return result.Contains($"{KeyWords.ReturnIdentifier} YES");
		}

		protected bool RunCommand(string command, IPattern fromPattern, IPattern toPattern, float commandParameter) {
			fromPattern.Validate();
			toPattern.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} YES\" if {command}({fromPattern.ToSikuliScript()},{toPattern.ToSikuliScript()}{ToSukuliFloat(commandParameter)}) else \"{KeyWords.ReturnIdentifier} NO\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			return result.Contains($"{KeyWords.ReturnIdentifier} YES");
		}

		protected string RunCommandWithReturn(string command, IPattern pattern, float commandParameter) {
			pattern.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} \" + getLastMatch().toString() if {command}({ pattern.ToSikuliScript()}{ ToSukuliFloat(commandParameter)}) else \"{KeyWords.ReturnIdentifier} None\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			Console.WriteLine(result);
			if (!result.Contains(KeyWords.ReturnIdentifier))
				throw new Exception("Command failed");
			return result;
		}

		protected string RunCommandWithMultiReturn(string command, IPattern pattern, float commandParameter) {
			pattern.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} \" + ''.join(map(str, getLastMatchs())) if {command}({ pattern.ToSikuliScript()}{ ToSukuliFloat(commandParameter)}) else \"{KeyWords.ReturnIdentifier} None\"";

			Console.WriteLine(script);
			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			Console.WriteLine(result);
			if (!result.Contains(KeyWords.ReturnIdentifier))
				throw new Exception("Command failed");
			return result;
		}

		protected string RunCommandWithMultiReturn(string command, params IPattern[] patterns) {
			foreach (var pattern in patterns) {
				pattern.Validate();
			}

			var script = $"print \"{KeyWords.ReturnIdentifier} \" + ''.join(map(str, getLastMatchs())) if {command}({string.Join(",", patterns.Select(p => p.ToSikuliScript()))}) else \"{KeyWords.ReturnIdentifier} None\"";

			Console.WriteLine(script);
			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, 0);
			Console.WriteLine(result);
			if (!result.Contains(KeyWords.ReturnIdentifier))
				throw new Exception("Command failed");
			return result;
		}

		protected string RunCommandWithMultiReturn(string command, float commandParameter, params IPattern[] patterns) {
			foreach (var pattern in patterns) {
				pattern.Validate();
			}

			var script = $"print \"{KeyWords.ReturnIdentifier} \" + ''.join(map(str, getLastMatchs())) if {command}({string.Join(",", patterns.Select(p => p.ToSikuliScript()))}{ToSukuliFloat(commandParameter)}) else \"{KeyWords.ReturnIdentifier} None\"";

			Console.WriteLine(script);
			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, 0);
			Console.WriteLine(result);
			if (!result.Contains(KeyWords.ReturnIdentifier))
				throw new Exception("Command failed");
			return result;
		}

		#endregion
		#region IRegion
		protected bool RunDotCommand(string command, IRegion region, string paramString, float commandParameter) {
			region.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} YES\" if {region.ToSikuliScript()}.{command}({paramString}) else \"{KeyWords.ReturnIdentifier} NO\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			return result.Contains($"{KeyWords.ReturnIdentifier} YES");
		}

		protected bool RunCommand(string command, IRegion region, float commandParameter) {
			region.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} YES\" if {command}({region.ToSikuliScript()}) else \"{KeyWords.ReturnIdentifier} NO\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			return result.Contains($"{KeyWords.ReturnIdentifier} YES");
		}

		protected bool RunCommand(string command, IRegion fromRegion, IRegion toRegion, float commandParameter) {
			fromRegion.Validate();
			toRegion.Validate();

			var script = $"print \"{KeyWords.ReturnIdentifier} YES\" if {command}({fromRegion.ToSikuliScript()},{toRegion.ToSikuliScript()}) else \"{KeyWords.ReturnIdentifier} NO\"";

			var result = _runtime.Run(script, KeyWords.ReturnIdentifier, commandParameter * 1.5d); // Failsafe
			return result.Contains($"{KeyWords.ReturnIdentifier} YES");
		}
		#endregion
		#endregion

		private static string ToSukuliFloat(float timeoutInSeconds) {
			return timeoutInSeconds > 0f ? ", " + timeoutInSeconds.ToString("0.####") : "";
		}

		public void Dispose() {
			_runtime.Stop();
		}
	}
}
