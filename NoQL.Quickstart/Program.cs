using System;
using NoQL.CEP.NewExpressions;

using NoQL.CEP;

namespace NoQL.Quickstart
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var p = new Processor(2,1);
		

			//Yolo.Manager.Register("example1", 0);
			var expr1 = Yolo.Express<int>().Where(x => x > 5).Perform(x => System.Console.WriteLine(x));
			//.Manager.Get("example1").Attach(expr1);
			expr1.Send(10);

			expr1.Send(4);


		}
	}
}
