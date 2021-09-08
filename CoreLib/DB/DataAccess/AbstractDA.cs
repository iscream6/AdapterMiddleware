using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmDA.DataAccess
{
    public abstract class AbstractDA
    {
        /// <summary>
        /// Transaction 개체를 얻는다.
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract IDbTransaction GetTransaction();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract bool IsAvaliable();

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery);

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, bool pbThrowException);
        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters);

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, bool pbThrowException);

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poTransaction"></param>
        /// <returns></returns>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbTransaction poTransaction);

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poTransaction"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbTransaction poTransaction, bool pbThrowException);


        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction);

        /// <summary>
        /// 쿼리를 수행하여 쿼리에 대해 영향을 받은 row count를 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException);


        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <history>
        ///     <revision  date="2005/05/19">신규작성</revision>
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery);

        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, bool pbThrowException);


        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters);

        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, bool pbThrowException);


        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poTransaction"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbTransaction poTransaction);

        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poTransaction"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbTransaction poTransaction, bool pbThrowException);


        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction);

        /// <summary>
        /// 쿼리를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psQuery"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException);


        /// <summary>
        /// stored procedure를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psProcedureName"></param>
        /// <param name="poParameters"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters);

        /// <summary>
        /// stored procedure를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psProcedureName"></param>
        /// <param name="poParameters"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, bool pbThrowException);


        /// <summary>
        /// stored procedure를 수행하여 해당 결과를 DataTable로 반환한다.
        /// </summary>
        /// <param name="psProcedureName"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <history>
        ///     <revision  date="2020/12/16">신규작성</revision>
        /// </history>
        public abstract int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, IDbTransaction poTransaction);

        /// <summary>
        /// stored procedure를 수행하여 해당 결과를 DataTable로 반환한다.
        /// Throw Exception 유무선택
        /// </summary>
        /// <param name="psProcedureName"></param>
        /// <param name="poParameters"></param>
        /// <param name="poTransaction"></param>
        /// <param name="pbThrowException"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[이재영]	2020-12-16	Created
        /// </history>
        public abstract int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException);
    }
}
