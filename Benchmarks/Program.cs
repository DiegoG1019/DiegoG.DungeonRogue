using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;
					
public static class Program
{
	public static List<object> TypeCheckList { get; } = new();
	public static List<A19> KnownTypeList { get; } = new();
	
	public static void Main()
	{
		for (int ix = 0; ix < 1000; ix++)
		{
			TypeCheckList.Add(new A0());
			TypeCheckList.Add(new A1());
			TypeCheckList.Add(new A2());
			TypeCheckList.Add(new A3());
			TypeCheckList.Add(new A4());
			TypeCheckList.Add(new A5());
			TypeCheckList.Add(new A6());
			TypeCheckList.Add(new A7());
			TypeCheckList.Add(new A8());
			TypeCheckList.Add(new A9());
			TypeCheckList.Add(new A10());
			TypeCheckList.Add(new A11());
			TypeCheckList.Add(new A12());
			TypeCheckList.Add(new A13());
			TypeCheckList.Add(new A14());
			TypeCheckList.Add(new A15());
			TypeCheckList.Add(new A16());
			TypeCheckList.Add(new A17());
			TypeCheckList.Add(new A18());
			TypeCheckList.Add(new A19());
			TypeCheckList.Add(new A20());
			TypeCheckList.Add(new A21());
			TypeCheckList.Add(new A22());
			TypeCheckList.Add(new A23());
			TypeCheckList.Add(new A24());
			TypeCheckList.Add(new A25());
			TypeCheckList.Add(new A26());
			TypeCheckList.Add(new A27());
			TypeCheckList.Add(new A28());
			TypeCheckList.Add(new A29());
			TypeCheckList.Add(new A30());
			
			for (int iy = 0; iy < 30; iy++)
			{
				KnownTypeList.Add(new A19());
			}
		}
		
		Debug.Assert(KnownTypeList.Count == TypeCheckList.Count);

		BenchmarkRunner.Run<testo>();
		
		return;
		var inter = "ABCDEFGHI";
		var set = new HashSet<char>();
		var sb = new StringBuilder(300);
		
		for (int i = 0; i < 31; i++)
		{
			var amount = RandomNumberGenerator.GetInt32(-9, 10);
			Console.Write($"{i} ::: ");
			Console.Write(amount);
			Console.Write("    ");
			if (amount != 0) amount = int.Abs(amount);
			Console.Write(amount);
			
			set.Clear();
			for (int x = 0; x < amount; x++)
			{
				for (int y = 0; i < 30; y++)
				{
					var which = RandomNumberGenerator.GetInt32(0, 9);
					if (set.Add(inter[which])) { Console.Write($" [ Added {inter[which]} ({which}) ]"); break; } 
				}
			}
			
			sb.Append("public class A").Append(i).Append(" : TestBase ");
			if(set.Count > 0)
			{
				Console.Write(" !! Non-zero on interfaces");
				sb.Append(", ");
				foreach(var @interface in set)
				{
					sb.Append(' ').Append(@interface).Append(',');
				}
				
				sb.Remove(sb.Length - 1, 1);
			}
			
			sb.Append("{");
			if(set.Count > 0)
			{
				Console.Write(" !! Non-zero on methods");
				foreach(var @interface in set)
				{
					sb.Append(" public void ");
					sb.Append(@interface).Append("() {TestProp += 10;}");
				}
			}
			sb.AppendLine(" }");
			Console.WriteLine();
			
			set.Clear();
		}
		
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine(sb.ToString());
	}
}

public class testo
{
	[Benchmark]
	public void TestInterfaceChecks()
	{
		foreach (var obj in Program.TypeCheckList)
		{
			if (obj is A __A) __A.A();
			if (obj is B __B) __B.B();
			if (obj is C __C) __C.C();
			if (obj is D __D) __D.D();
			if (obj is E __E) __E.E();
			if (obj is F __F) __F.F();
			if (obj is G __G) __G.G();
			if (obj is H __H) __H.H();
			if (obj is I __I) __I.I();
		}
	}
	
	[Benchmark]
	public void TestKnownTypeChecks()
	{
		foreach (var obj in Program.KnownTypeList)
		{
			obj.A();
			obj.B();
			obj.C();
			obj.D();
			obj.E();
			obj.F();
			obj.G();
			obj.H();
			obj.I();
		}
	}
	
	//A19
}

public interface A { void A(); }
public interface B { void B(); }
public interface C { void C(); }
public interface D { void D(); }
public interface E { void E(); }
public interface F { void F(); }
public interface G { void G(); }
public interface H { void H(); }
public interface I { void I(); }

public class TestBase { public int TestProp { get; set; } }

public class A0 : TestBase ,  E, B, I, F, C{ public void E() {TestProp += 10;} public void B() {TestProp += 10;} public void I() {TestProp += 10;} public void F() {TestProp += 10;} public void C() {TestProp += 10;} }
public class A1 : TestBase { }
public class A2 : TestBase ,  B, C, A, D, H{ public void B() {TestProp += 10;} public void C() {TestProp += 10;} public void A() {TestProp += 10;} public void D() {TestProp += 10;} public void H() {TestProp += 10;} }
public class A3 : TestBase ,  C, H, G, A, E, D, B, F{ public void C() {TestProp += 10;} public void H() {TestProp += 10;} public void G() {TestProp += 10;} public void A() {TestProp += 10;} public void E() {TestProp += 10;} public void D() {TestProp += 10;} public void B() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A4 : TestBase ,  D, I{ public void D() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A5 : TestBase ,  G, D, C, H, I, F{ public void G() {TestProp += 10;} public void D() {TestProp += 10;} public void C() {TestProp += 10;} public void H() {TestProp += 10;} public void I() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A6 : TestBase ,  G, C, D, A, H, F, B{ public void G() {TestProp += 10;} public void C() {TestProp += 10;} public void D() {TestProp += 10;} public void A() {TestProp += 10;} public void H() {TestProp += 10;} public void F() {TestProp += 10;} public void B() {TestProp += 10;} }
public class A7 : TestBase ,  G, F, H, C, I{ public void G() {TestProp += 10;} public void F() {TestProp += 10;} public void H() {TestProp += 10;} public void C() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A8 : TestBase ,  G, E, D, I, H, F{ public void G() {TestProp += 10;} public void E() {TestProp += 10;} public void D() {TestProp += 10;} public void I() {TestProp += 10;} public void H() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A9 : TestBase ,  E, D, B, F, C, I{ public void E() {TestProp += 10;} public void D() {TestProp += 10;} public void B() {TestProp += 10;} public void F() {TestProp += 10;} public void C() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A10 : TestBase ,  F, E, H, D, G, A{ public void F() {TestProp += 10;} public void E() {TestProp += 10;} public void H() {TestProp += 10;} public void D() {TestProp += 10;} public void G() {TestProp += 10;} public void A() {TestProp += 10;} }
public class A11 : TestBase ,  G, I, H, E, D, B, C, A, F{ public void G() {TestProp += 10;} public void I() {TestProp += 10;} public void H() {TestProp += 10;} public void E() {TestProp += 10;} public void D() {TestProp += 10;} public void B() {TestProp += 10;} public void C() {TestProp += 10;} public void A() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A12 : TestBase ,  G, D{ public void G() {TestProp += 10;} public void D() {TestProp += 10;} }
public class A13 : TestBase ,  G, D, F{ public void G() {TestProp += 10;} public void D() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A14 : TestBase ,  G, B, H, I, C, E, D{ public void G() {TestProp += 10;} public void B() {TestProp += 10;} public void H() {TestProp += 10;} public void I() {TestProp += 10;} public void C() {TestProp += 10;} public void E() {TestProp += 10;} public void D() {TestProp += 10;} }
public class A15 : TestBase ,  B, F, E{ public void B() {TestProp += 10;} public void F() {TestProp += 10;} public void E() {TestProp += 10;} }
public class A16 : TestBase ,  B, A, D, H, E, C, F{ public void B() {TestProp += 10;} public void A() {TestProp += 10;} public void D() {TestProp += 10;} public void H() {TestProp += 10;} public void E() {TestProp += 10;} public void C() {TestProp += 10;} public void F() {TestProp += 10;} }
public class A17 : TestBase ,  E, H, F, I, B, A{ public void E() {TestProp += 10;} public void H() {TestProp += 10;} public void F() {TestProp += 10;} public void I() {TestProp += 10;} public void B() {TestProp += 10;} public void A() {TestProp += 10;} }
public class A18 : TestBase ,  G, C, H, A, D, E, I{ public void G() {TestProp += 10;} public void C() {TestProp += 10;} public void H() {TestProp += 10;} public void A() {TestProp += 10;} public void D() {TestProp += 10;} public void E() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A19 : TestBase ,  D, I, F, H, G, A, E, B, C{ public void D() {TestProp += 10;} public void I() {TestProp += 10;} public void F() {TestProp += 10;} public void H() {TestProp += 10;} public void G() {TestProp += 10;} public void A() {TestProp += 10;} public void E() {TestProp += 10;} public void B() {TestProp += 10;} public void C() {TestProp += 10;} }
public class A20 : TestBase ,  C{ public void C() {TestProp += 10;} }
public class A21 : TestBase ,  F, I{ public void F() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A22 : TestBase ,  E, F, H, C, A, G, I, D, B{ public void E() {TestProp += 10;} public void F() {TestProp += 10;} public void H() {TestProp += 10;} public void C() {TestProp += 10;} public void A() {TestProp += 10;} public void G() {TestProp += 10;} public void I() {TestProp += 10;} public void D() {TestProp += 10;} public void B() {TestProp += 10;} }
public class A23 : TestBase ,  B, F, H, A{ public void B() {TestProp += 10;} public void F() {TestProp += 10;} public void H() {TestProp += 10;} public void A() {TestProp += 10;} }
public class A24 : TestBase { }
public class A25 : TestBase ,  A, E, I{ public void A() {TestProp += 10;} public void E() {TestProp += 10;} public void I() {TestProp += 10;} }
public class A26 : TestBase ,  F, A{ public void F() {TestProp += 10;} public void A() {TestProp += 10;} }
public class A27 : TestBase ,  A, D, C, F, B{ public void A() {TestProp += 10;} public void D() {TestProp += 10;} public void C() {TestProp += 10;} public void F() {TestProp += 10;} public void B() {TestProp += 10;} }
public class A28 : TestBase ,  C, B, G{ public void C() {TestProp += 10;} public void B() {TestProp += 10;} public void G() {TestProp += 10;} }
public class A29 : TestBase ,  G, H, F, D, B, C, A{ public void G() {TestProp += 10;} public void H() {TestProp += 10;} public void F() {TestProp += 10;} public void D() {TestProp += 10;} public void B() {TestProp += 10;} public void C() {TestProp += 10;} public void A() {TestProp += 10;} }
public class A30 : TestBase { }