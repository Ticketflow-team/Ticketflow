# Threat Model as Code — regeneration des artefacts (DFD, sequence, rapport).
# Prerequis : python3 + pytm (venv), graphviz (dot), java + plantuml.
#
#   python3 -m venv .venv-tm && source .venv-tm/bin/activate && pip install pytm
#   sudo apt install graphviz default-jre plantuml
#
# Les artefacts generes vont dans out/ (ignore par Git).

PY        ?= python3
PLANTUML  ?= plantuml            # ou : java -jar /chemin/plantuml.jar
OUT       := out

.PHONY: all dfd seq report clean

all: dfd seq report

$(OUT):
	mkdir -p $(OUT)

# Data Flow Diagram
dfd: $(OUT)
	$(PY) tm.py --dfd | dot -Tpng -o $(OUT)/dfd.png
	@echo "-> $(OUT)/dfd.png"

# Diagramme de sequence
seq: $(OUT)
	$(PY) tm.py --seq | $(PLANTUML) -tpng -pipe > $(OUT)/seq.png
	@echo "-> $(OUT)/seq.png"

# Rapport des menaces candidates (moteur pytm — a consolider dans threats.md)
report: $(OUT)
	$(PY) tm.py --report report-template.md > $(OUT)/report.md
	@echo "-> $(OUT)/report.md"

clean:
	rm -rf $(OUT)
