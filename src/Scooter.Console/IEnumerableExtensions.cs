using System;
using System.Collections.Generic;
using System.Linq;

namespace Scooter
{
	/// <summary>
	///     from http://stackoverflow.com/a/1653204/102536
	/// </summary>
	public static class IEnumerableExtensions
	{
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			return source.Shuffle(new Random());
		}

		public static IEnumerable<T> Shuffle<T>(
			this IEnumerable<T> source, Random rng)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (rng == null)
			{
				throw new ArgumentNullException(nameof(rng));
			}

			return source.ShuffleIterator(rng);
		}

		private static IEnumerable<T> ShuffleIterator<T>(
			this IEnumerable<T> source, Random random)
		{
			var buffer = source.ToList();
			for (var i = 0; i < buffer.Count; i++)
			{
				var j = random.Next(i, buffer.Count);
				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}
	}
}