﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OEP.Core.DomainModels.ExamModels;
using OEP.Data;
using OEP.Core.Services;
using AutoMapper;
using OEP.Resources.Admin;
using Microsoft.AspNet.Identity;
using OEP.Core.Data;
using OEP.Web.Helpers;

namespace OEP.Web.Areas.Admin.Controllers
{
    [AuthorizeUser(Roles = "Admin,Faculty")]
    public class ExamTypesController : Controller
    {
        private readonly IExamTypeService _examTypeService;

        public ExamTypesController(IExamTypeService examTypeService)
        {
            _examTypeService = examTypeService;
        }
        // GET: Admin/ExamTypes
        public ActionResult Index()
        {
            return View();
        }

        // POST: /Admin/ExamTypes/LoadExamTypes
        public async Task<ActionResult> LoadExamTypes()
        {
            try
            {
                if (Request.Form != null)
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn =
                        Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();


                    //Paging Size (10,20,50,100)    
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 1;
                    int recordsTotal = 0;

                    var examTypeList = await _examTypeService.GetAllAsync(
                        skip,
                        pageSize,

                        //sorting
                        x => sortColumn == "Name" ? x.Name : null,

                        //filtering
                        x => searchValue != "" ? x.Name.Contains(searchValue) : x.Id != 0,

                        //sort by
                        (sortColumnDir == "desc" ? OrderBy.Descending : OrderBy.Ascending)
                     );

                    var resp = Mapper.Map<List<ExamType>, List<ExamTypeResource>>(examTypeList);

                    //total number of rows count     
                    recordsTotal = examTypeList.TotalCount;
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = resp });
                }
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
            return Content("Error");
        }



        // GET: Admin/ExamTypes/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamType examType = await _examTypeService.GetByIdAsync(Convert.ToInt32(id));
            if (examType == null)
            {
                return HttpNotFound();
            }
            var examtypeResource = Mapper.Map<ExamType, ExamTypeResource>(examType);
            return View(examtypeResource);
        }

        // GET: Admin/ExamTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/ExamTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ExamTypeResource examTypeResource)
        {
            if (ModelState.IsValid)
            {
                var examType = Mapper.Map<ExamTypeResource, ExamType>(examTypeResource);
                examType.CreatedDate = DateTime.Now;
                examType.UpdatedDate = DateTime.Now;
                var userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                examType.UserId = userId;

                await _examTypeService.AddAsync(examType);
                _examTypeService.UnitOfWorkSaveChanges();

                return RedirectToAction("Index");
            }

            return View(examTypeResource);
        }

        // GET: Admin/ExamTypes/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExamType examType = await _examTypeService.GetByIdAsync(Convert.ToInt32(id));
            if (examType == null)
            {
                return HttpNotFound();
            }
            var examtypeResource = Mapper.Map<ExamType, ExamTypeResource>(examType);
            return View(examtypeResource);
        }

        // POST: Admin/ExamTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ExamTypeResource examTypeResource)
        {
            if (ModelState.IsValid)
            {
                var exstexamType = await _examTypeService.GetByIdAsync(examTypeResource.Id);
                exstexamType.Name = examTypeResource.Name;
                exstexamType.UpdatedDate = DateTime.Now;
                exstexamType.Status = examTypeResource.Status;
                var userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                exstexamType.UserId = userId;
                await _examTypeService.UpdateAsync(exstexamType);
                _examTypeService.UnitOfWorkSaveChanges();


                return RedirectToAction("Index");
            }
            return View(examTypeResource);
        }

        // GET: Admin/ExamTypes/Delete/5
        [AuthorizeUser(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            ExamType examType = await _examTypeService.GetByIdAsync(Convert.ToInt32(id));
            if (examType == null)
            {
                return HttpNotFound();
            }
            var examtypeResource = Mapper.Map<ExamType, ExamTypeResource>(examType);
            return View(examtypeResource);
        }

        // POST: Admin/ExamTypes/Delete/5
        [AuthorizeUser(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            ExamType examType = await _examTypeService.GetByIdAsync(id);
            await _examTypeService.DeleteAsync(examType);
            _examTypeService.UnitOfWorkSaveChanges();


            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _examTypeService.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
