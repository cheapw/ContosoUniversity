using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
//using System.Diagnostics;

namespace ContosoUniversity.Controllers
{
    public enum SortOrder
    {
        @default,lastName_desc,   // 根据姓氏正反序排列
        firstName_asc,firstNaem_desc,   // 根据名字正反序排列
        date_asc,date_desc  // 根据日期正反序排列
    }
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Students

        #region 旧的Index方法，使用字符串参数
        //public async Task<IActionResult> Index(string sortOrder = "default")
        //{
        //    ViewData["LastNameSortParm"] = sortOrder == "default" ? "lastName_desc" : "default";
        //    ViewData["FirstNameSortParm"] = sortOrder == "firstName_asc" ? "firstName_desc" : "firstName_asc";
        //    ViewData["DateSortParm"] = sortOrder == "date_asc" ? "date_desc" : "date_asc";
        //    var students = from s in _context.Students
        //                   select s;
        //    switch (sortOrder)
        //    {
        //        case "default":
        //            students = students.OrderBy(s => s.LastName);
        //            break;
        //        case "lastName_desc":
        //            students = students.OrderByDescending(s => s.LastName);
        //            break;
        //        case "firstName_asc":
        //            students = students.OrderBy(s => s.FirstMidName);
        //            break;
        //        case "firstName_desc":
        //            students = students.OrderByDescending(s => s.FirstMidName);
        //            break;
        //        case "date_asc":
        //            students = students.OrderBy(s => s.EnrollmentDate);
        //            break;
        //        case "date_desc":
        //            students = students.OrderByDescending(s => s.EnrollmentDate);
        //            break;
        //        default:
        //            throw new Exception("排序时内部数据错误！");
        //    }
        //    return View(await students.ToListAsync());
        //} 
        #endregion

        #region 新的Index方法，使用枚举参数
        public async Task<IActionResult> Index(string searchString,string currentFilter,int? page,SortOrder sortOrder = SortOrder.@default)
        {
            ViewData["LastNameSortParm"] = sortOrder == SortOrder.@default ? SortOrder.lastName_desc : SortOrder.@default;
            ViewData["FirstNameSortParm"] = sortOrder == SortOrder.firstName_asc ? SortOrder.firstNaem_desc : SortOrder.firstName_asc;
            ViewData["DateSortParm"] = sortOrder == SortOrder.date_asc ? SortOrder.date_desc : SortOrder.date_asc;
            ViewData["CurrentSort"] = sortOrder;

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var students = from s in _context.Students
                           select s;
            if (!string.IsNullOrEmpty(searchString))
            {
                students=students.Where(s => s.LastName.Contains(searchString) || s.FirstMidName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case SortOrder.@default:
                    students = students.OrderBy(s => s.LastName);
                    break;
                case SortOrder.lastName_desc:
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case SortOrder.firstName_asc:
                    students = students.OrderBy(s => s.FirstMidName);
                    break;
                case SortOrder.firstNaem_desc:
                    students = students.OrderByDescending(s => s.FirstMidName);
                    break;
                case SortOrder.date_asc:
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case SortOrder.date_desc:
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    throw new Exception("排序时内部数据错误！");
            }
            int pageSize = 3;

            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), page ?? 1, pageSize));
        } 
        #endregion

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s=>s.Enrollments).ThenInclude(e=>e.Course).AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            //if (student.Enrollments == null)
            //{
            //    Debug.WriteLine("Enrollments为null!");

            //}
            //else
            //{
            //    Debug.WriteLine("Enrollments不为null");
            //}

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LastName,FirstName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                // Log the error (uncommnet ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator. The error message is"+ex.Message);
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost,ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studentToUpdate = await _context.Students.SingleOrDefaultAsync(s => s.ID == id);
            if(await TryUpdateModelAsync<Student>(studentToUpdate, "", s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    // Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

            //if (ModelState.IsValid)
            //{
            //    try
            //    {
            //        _context.Update(student);
            //        await _context.SaveChangesAsync();
            //    }
            //    catch (DbUpdateConcurrencyException)
            //    {
            //        if (!StudentExists(student.ID))
            //        {
            //            return NotFound();
            //        }
            //        else
            //        {
            //            throw;
            //        }
            //    }
            //    return RedirectToAction(nameof(Index));
            //}

            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id,bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);

            if (student==null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                // Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
