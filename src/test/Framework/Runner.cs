using System;
using System.Reflection;
using System.Collections.Generic;

using Neo.Utils;

namespace Neo.Tests.Framework {
	internal sealed class TestFailure {
		public TestFailure(string fixture, string name, Exception error) {
			Fixture = fixture;
			Name = name;
			Error = error;
		}

		public string Fixture { get; }

		public string Name { get; }
		
		public Exception Error { get; }
	}

	public static class Runner {
		public static void Main() {
			var failures = new List<TestFailure>();
			var passed = 0;
			var skipped = 0;

			int addFailure(string fixture, string name, Exception error) {
				failures.Add(new TestFailure(fixture, name, error));
				return failures.Count;
			}

			void addPass() {
				passed++;
			}

			var assembly = Assembly.GetExecutingAssembly();
			foreach(var type in assembly.GetTypes()) {
				var testFixture = type.FindCustomAttribute<TestFixture>();
				if (testFixture == null) continue;

				if (testFixture.Enabled) {
					Console.WriteLine(type.Name);
				} else {
					Console.WriteLine($"{type.Name} skipped");
				}

				foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
					var test = method.FindCustomAttribute<Test>();
					if (test == null) continue;

					if(testFixture.Enabled) {
						if (!test.Enabled) {
							Console.WriteLine($"    \u001B[33mx\u001B[0m {method.Name}");
							skipped++;
							continue;
						}

						object instance;
						
						try {
							instance = Activator.CreateInstance(type);
						} catch(Exception e) {
							throw e; // @TODO: handle this case
						}

						try {
							method.Invoke(instance, BindingFlags.Default, null, new object[0], null);
							addPass();

							Console.WriteLine($"    \u001B[32m\u2714\u001b[0m {method.Name}");
						} catch (TargetInvocationException e) {
							var idx = addFailure(type.Name, method.Name, e.InnerException);

							Console.WriteLine($"    \u001B[31m{idx})\u001b[0m {method.Name}");
						}
					} else {
						skipped++;
					}
				}
			}

			Console.WriteLine();

			for(var i = 0; i < failures.Count; i++) {
				var f = failures[i];

				Console.WriteLine($"\u001b[31m{i+1})\u001b[0m {f.Fixture}.{f.Name}");

				var error = f.Error.ToString();
				foreach(var line in error.Split("\n")) {
					Console.WriteLine($"    {line}");
				}

				Console.WriteLine();
			}

        	var failed = failures.Count;
        	var total = failed + passed + skipped;

        	string percent(int n) {
        		if(total == 0) return "0";
        		
        		var x = (double) n / (double) total * 100.0;
        		return x.ToString("N2");
        	}

        	Console.WriteLine($"\u001B[33m{skipped} skipped ({percent(skipped)}%)\u001B[0m");
        	Console.WriteLine($"\u001B[31m{failed} failing ({percent(failed)}%)\u001B[0m");
        	Console.WriteLine($"\u001B[32m{passed} passing ({percent(passed)}%)\u001B[0m");

        	if(failed > 0) Environment.Exit(1);
		}
	}
}