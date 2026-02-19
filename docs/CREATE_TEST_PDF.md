# How to Create a Test PDF for Contract Intelligence

I've prepared sample contract content for you to test the application. Here are **three easy ways** to create a PDF:

## ⚡ Quick Option 1: Use Google Docs (Easiest)

1. Open [Google Docs](https://docs.google.com)
2. Create a new document
3. Copy all content from `sample_contract_text.txt`
4. Paste it into the Google Doc
5. Go to **File → Download → PDF Document (.pdf)**
6. Done! You now have `sample_contract_text.pdf`

## 📄 Quick Option 2: Use Microsoft Word

1. Open Microsoft Word
2. Create a new document
3. Copy all content from `sample_contract_text.txt`
4. Paste it into Word
5. Go to **File → Save As → Save as type: PDF**
6. Save as `sample_contract.pdf`

## 🌐 Quick Option 3: Use Online Converter

1. Go to [PDF.io](https://pdf.io/txt-to-pdf/) or [Smallpdf](https://smallpdf.com/txt-to-pdf)
2. Upload `sample_contract_text.txt`
3. Click "Convert to PDF"
4. Download the generated PDF

## 🐍 Advanced Option: Python Script (If You Have Python)

If you have Python installed with pip:

```bash
# Install required package
pip install reportlab

# Run the generator script
python generate_sample_contract.py
```

This will create a professionally formatted `sample_software_license.pdf`.

## What Makes This Contract Perfect for Testing?

The sample contract includes **all clause types** the system can detect:

✅ **Renewal Clauses** (Section 2.1)
- "This Agreement shall have an initial term of three (3) years"
- System will detect: `renewal` clause type

✅ **Auto-Renewal Provisions** (Section 2.2)
- "this Agreement shall automatically renew for successive one-year renewal periods"
- System will detect: `auto_renewal` clause type

✅ **Termination Clauses** (Section 3)
- "terminate this Agreement upon written notice"
- "termination clause shall take effect"
- System will detect: `termination` clause type

✅ **Data Protection Requirements** (Section 5)
- "GDPR compliance", "data protection laws", "confidential information"
- "information security measures", "personal data"
- System will detect: `data_protection` clause type

✅ **Liability Caps** (Section 6)
- "TOTAL LIABILITY... SHALL NOT EXCEED"
- "liability limitation", "indemnity cap"
- System will detect: `liability_cap` clause type

✅ **Governing Law** (Section 7)
- "governed by and construed in accordance with the laws"
- "jurisdiction clause", "submit to the exclusive jurisdiction"
- System will detect: `governing_law` clause type

## Testing the Application

Once you have your PDF:

1. **Go to the app** at http://localhost:5173
2. **Click "Create Contract"** button (top-right)
3. **Fill in the form:**
   - Title: `Software License Agreement`
   - Vendor: `TechVendor Solutions Inc.`
   - Start Date: `2025-01-15`
   - End Date: `2028-01-14`
   - Renewal Date: `2028-01-14`
   - Status: `Active`
4. **Click "Create Contract"**
5. You'll be redirected to the contract detail page
6. **Upload your PDF** using the file selector
7. **Click "Upload"** button
8. **Click "🔍 Run Extraction"** button
9. Watch the magic happen! ✨

## Expected Results

After extraction completes, you should see:

- **~10-15 detected clauses** (depending on the regex matches)
- **Clause types identified:**
  - 2-3 Renewal clauses
  - 1-2 Auto-renewal clauses
  - 2-3 Termination clauses
  - 3-4 Data protection clauses
  - 2-3 Liability cap clauses
  - 2-3 Governing law clauses

- **Risk Score:** Calculated based on detected clauses
- **Confidence levels:** Between 0.70 and 0.85 for each clause
- **Page numbers:** Showing which page each clause appears on
- **Excerpts:** Short snippets of text showing the matched content

## Troubleshooting

### If extraction fails:

1. **Verify your PDF is text-based:**
   - Open the PDF
   - Try to select text with your mouse
   - If you can select text → ✅ Good
   - If you cannot select text → ❌ Image-based PDF

2. **Check the error message:**
   - The improved error handling will tell you exactly what went wrong
   - Look for messages like "No text extracted" or "File not found"

3. **Try recreating the PDF:**
   - Use Google Docs (most reliable for text-based PDFs)
   - Make sure you're downloading as PDF, not printing to PDF

### The sample_contract.pdf in storage

There's already a `sample_contract.pdf` in your storage folder (`src/WebApi/storage/174c5947-afd1-49c0-af3b-505b04557d03/`). However, this is a minimal demo file. The contract from `sample_contract_text.txt` is much more comprehensive and will give you better test results!

## What Happens During Extraction?

1. **Text Extraction** - PdfPig library reads all text from the PDF
2. **Clause Detection** - Regex patterns scan for specific clause types
3. **Page Mapping** - System identifies which page each clause is on
4. **Risk Scoring** - Calculates overall contract risk based on clauses found
5. **Database Update** - Saves all detected clauses and updates the contract

The whole process typically takes less than 1 second for a 2-3 page contract.

## Need More Test Files?

You can create variations:

- **High-risk contract:** Add more termination and liability clauses
- **Simple contract:** Remove sections to test with minimal clauses
- **Multi-page contract:** Add more sections to test pagination
- **Different formats:** Try different fonts, spacing, headers

Just remember: **Always use text-based PDFs**, not scans or images!

---

**Pro Tip:** Keep `sample_contract_text.txt` handy. You can modify it and regenerate PDFs to test different scenarios and edge cases!
