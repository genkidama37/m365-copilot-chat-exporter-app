(function () {
  function initQuizzes() {
    var cards = document.querySelectorAll('[data-quiz-card]');
    cards.forEach(function (card) {
      var buttons = card.querySelectorAll('[data-option]');
      var feedback = card.querySelector('[data-feedback]');
      buttons.forEach(function (btn) {
        btn.addEventListener('click', function () {
          buttons.forEach(function (b) {
            b.classList.remove('correct', 'incorrect');
          });
          var isCorrect = btn.getAttribute('data-correct') === 'true';
          btn.classList.add(isCorrect ? 'correct' : 'incorrect');
          if (feedback) {
            feedback.textContent = btn.getAttribute('data-explanation') || '';
            feedback.className = 'feedback-banner visible ' + (isCorrect ? 'ok' : 'err');
          }
        });
      });
    });
  }

  function initDiffSpotters() {
    var boxes = document.querySelectorAll('[data-diff-spotter]');
    boxes.forEach(function (box) {
      var lines = box.querySelectorAll('[data-diff-line]');
      var feedback = box.querySelector('[data-diff-feedback]');
      lines.forEach(function (line) {
        line.addEventListener('click', function () {
          lines.forEach(function (l) {
            l.classList.remove('correct', 'incorrect');
          });
          var isBug = line.getAttribute('data-bug') === 'true';
          line.classList.add(isBug ? 'correct' : 'incorrect');
          if (feedback) {
            feedback.textContent = line.getAttribute('data-explanation') || '';
            feedback.className = 'feedback-banner visible ' + (isBug ? 'ok' : 'err');
          }
        });
      });
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
      initQuizzes();
      initDiffSpotters();
    });
  } else {
    initQuizzes();
    initDiffSpotters();
  }
})();
