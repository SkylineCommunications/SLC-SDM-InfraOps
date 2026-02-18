namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	public sealed class AssetClassLifecycle : IEquatable<AssetClassLifecycle>
	{
		public DateTime EndOfLife { get; set; }

		public DateTime EndOfService { get; set; }

		public TimeSpan NominalLifetime { get; set; }

		public static bool operator ==(AssetClassLifecycle left, AssetClassLifecycle right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			if (left is null || right is null)
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(AssetClassLifecycle left, AssetClassLifecycle right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetClassLifecycle);
		}

		public bool Equals(AssetClassLifecycle other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return
				Nullable.Equals(EndOfLife, other.EndOfLife) &&
				Nullable.Equals(EndOfService, other.EndOfService) &&
				Nullable.Equals(NominalLifetime, other.NominalLifetime);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + EndOfLife.GetHashCode();
				hash = (hash * 23) + EndOfService.GetHashCode();
				hash = (hash * 23) + NominalLifetime.GetHashCode();
				return hash;
			}
		}
	}
}