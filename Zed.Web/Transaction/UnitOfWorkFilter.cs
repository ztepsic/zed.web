using System;
using System.Web.Mvc;
using Zed.Transaction;

namespace Zed.Web.Transaction {
    /// <summary>
    /// ASP.NET MVC Unit of Work action filter
    /// </summary>
    public class UnitOfWorkFilter : IActionFilter {

        #region Fields and Properties

        /// <summary>
        /// Order of attribute in filter attribute stack.
        /// The number needs to be as high as possible because we want to delay
        /// the session and transaction opening to the last moment.
        /// </summary>
        public const int ORDER_OF_FILTER_IN_STACK_OF_FILTERS = 100;

        /// <summary>
        /// Unit of work
        /// </summary>
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        /// Current unit of work scope
        /// </summary>
        private IUnitOfWorkScope currentUnitOfWorkScope;

        /// <summary>
        /// Gets or Sets indication should we rollback on model state error
        /// </summary>
        public bool RollbackOnModelStateError { get; set; }

        #endregion

        #region Constructors and Init

        /// <summary>
        /// Creates Unit of Work attribute instance
        /// </summary>
        /// <param name="unitOfWork">Unit of work</param>
        public UnitOfWorkFilter(IUnitOfWork unitOfWork) {
            this.unitOfWork = unitOfWork;
            RollbackOnModelStateError = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The method begins with transaction
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        public void OnActionExecuting(ActionExecutingContext filterContext) {
            currentUnitOfWorkScope = unitOfWork.Start();
        }

        /// <summary>
        /// The method commits or rollbacks ongoing transaction.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        public void OnActionExecuted(ActionExecutedContext filterContext) {
            if (currentUnitOfWorkScope != null) {
                try {
                    if ((filterContext.Exception != null) && (!filterContext.ExceptionHandled) || shouldRollback(filterContext)) {
                        currentUnitOfWorkScope.Rollback();
                    } else {
                        currentUnitOfWorkScope.Commit();
                    }
                } catch (Exception) {
                    currentUnitOfWorkScope.Rollback();
                } finally {
                    currentUnitOfWorkScope.Dispose();
                }
            }
        }


        private bool shouldRollback(ControllerContext filterContext) {
            return RollbackOnModelStateError && !filterContext.Controller.ViewData.ModelState.IsValid;
        }

        #endregion

    }
}
