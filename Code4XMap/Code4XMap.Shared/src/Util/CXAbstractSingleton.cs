using System;
using System.Collections.Generic;
using System.Text;

namespace Code4XMap
{
	/// <summary>
	/// 抽象Singletonクラス
	/// </summary>
	public abstract class CXAbstractSingleton : MarshalByRefObject 
	{
		#region メンバ
		/// <summary>
		/// 唯一のインスタンスを保持するためのHashtable
		/// </summary>
		private static Dictionary<Type, CXAbstractSingleton> dicSingletonInstance = new Dictionary<Type, CXAbstractSingleton>();

		/// <summary>
		/// lockオブジェクト
		/// </summary>
		private static readonly object lockObject = new object();
		#endregion

		#region コンストラクタ
		/// <summary>
		/// コンストラクタ
		/// サブクラスのコンストラクタのアクセシビリティはprotectedに設定してください。
		/// </summary>
		protected CXAbstractSingleton()
		{
			if (dicSingletonInstance.ContainsKey(this.GetType()))
			{
				throw new System.InvalidOperationException("インスタンスは既に生成されています。\r\nAbstractSingletonClassのサブクラスのコンストラクタのアクセシビリティは\r\nprotectedにしてください。");
			}
			//Dictionaryに型ごとの唯一のインスタンスを保持
			dicSingletonInstance.Add(this.GetType(), this);
		}
		#endregion

		#region メソッド
		/// <summary>
		/// 指定の型のSingletonインスタンスを取得します。
		/// </summary>
		/// <typeparam name="T">
		/// 指定の型 T　：制約　AbstractSingletonClassのサブクラス
		/// </typeparam>
		/// <returns>指定の型のSingletonインスタンス</returns>
		public static T GetInstance<T>() where T : CXAbstractSingleton
		{
			lock (lockObject){
				// まだ作成されていない場合、インスタンスを生成
				if (!dicSingletonInstance.ContainsKey(typeof(T)))
				{
					// 指定された型のインスタンスを生成
					System.Activator.CreateInstance(typeof(T), true);
				}
				return (T)dicSingletonInstance[typeof(T)];
			}
		}
		#endregion
	}
}

