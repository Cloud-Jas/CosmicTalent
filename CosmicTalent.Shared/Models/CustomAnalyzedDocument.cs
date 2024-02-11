using Google.Protobuf.WellKnownTypes;

namespace CosmicTalent.Shared.Models
{
    public class CustomAnalyzedDocument
    {
        public string DocumentType { get; set; }
        public Fields Fields { get; set; }        
        public double Confidence { get; set; }
    }
    public class CustomTable
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<Cell> Cells { get; set; }
    }
    public class Cell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string Content { get; set; }
    }
    public class Table
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<Cell> Cells { get; set; }
    }

    public class EmployeeAchievements
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }                
        public double Confidence { get; set; }
    }

    public class EmployeeCertifications
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeExperienceHistory
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeId
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeName
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeProject1
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeProject2
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeProject3
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class EmployeeProject4
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }
    public class BoundingRegion
    {
        public int pageNumber { get; set; }
        public List<double> polygon { get; set; }
    }

    public class COLUMN1
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string Content { get; set; }
        public List<BoundingRegion> boundingRegions { get; set; }
        public List<Span> spans { get; set; }
    }

    public class COLUMN2
    {
        public string type { get; set; }
        public string valueString { get; set; }
        public string Content { get; set; }
        public List<BoundingRegion> boundingRegions { get; set; }
        public List<Span> spans { get; set; }
    }

    public class EmployeeSkills
    {
        public string type { get; set; }
        public List<ValueArray> ValueArray { get; set; }
    }

    public class Span
    {
        public int offset { get; set; }
        public int length { get; set; }
    }

    public class ValueArray
    {
        public string type { get; set; }
        public ValueObject ValueObject { get; set; }
    }

    public class ValueObject
    {
        public COLUMN1 COLUMN1 { get; set; }
        public COLUMN2 COLUMN2 { get; set; }
    }

    public class EmployeeSummary
    {
        public int FieldType { get; set; }
        public int ExpectedFieldType { get; set; }
        public Value Value { get; set; }
        public string Content { get; set; }
        public double Confidence { get; set; }
    }

    public class Fields
    {
        public EmployeeId EmployeeId { get; set; }
        public EmployeeName EmployeeName { get; set; }
        public EmployeeSummary EmployeeSummary { get; set; }
        public EmployeeCertifications EmployeeCertifications { get; set; }
        public EmployeeExperienceHistory EmployeeExperienceHistory { get; set; }
        public EmployeeProject1 EmployeeProject1 { get; set; }
        public EmployeeProject2 EmployeeProject2 { get; set; }
        public EmployeeProject3 EmployeeProject3 { get; set; }
        public EmployeeProject4 EmployeeProject4 { get; set; }
        public EmployeeAchievements EmployeeAchievements { get; set; }
        public EmployeeSkills EmployeeSkills { get; set; }
    }
}