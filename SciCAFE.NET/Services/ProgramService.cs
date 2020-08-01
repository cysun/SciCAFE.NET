using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SciCAFE.NET.Services
{
    public class ProgramService
    {
        private readonly AppDbContext _db;

        public ProgramService(AppDbContext db)
        {
            _db = db;
        }

        public List<Models.Program> GetPrograms()
        {
            return _db.Programs.ToList();
        }

        public Models.Program GetProgram(int id)
        {
            return _db.Programs.Find(id);
        }

        public void AddProgram(Models.Program program) => _db.Programs.Add(program);

        public void SaveChanges() => _db.SaveChanges();
    }
}
