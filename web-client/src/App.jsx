import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

function App() {
  const [quotes, setQuotes] = useState([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [currentQuote, setCurrentQuote] = useState(null);
  const [newQuote, setNewQuote] = useState({ content: '', author: '', tags: '' });
  const [tagSuggestions, setTagSuggestions] = useState([]);
  const [filterTag, setFilterTag] = useState('');
  const [showTopQuotes, setShowTopQuotes] = useState(false);
  
  const API_BASE_URL = 'http://localhost:5051/api/quotesapi';

  useEffect(() => {
    fetchQuotes();
  }, [currentPage, showTopQuotes, filterTag]);

  const fetchQuotes = async () => {
    try {
      let response;
      if (showTopQuotes) {
        response = await axios.get(`${API_BASE_URL}/top?count=10`);
      } else if (filterTag) {
        response = await axios.get(`${API_BASE_URL}/quotes_by_tag/${filterTag}`);
      } else {
        response = await axios.get(`${API_BASE_URL}?page=${currentPage}&pageSize=10`);
      }
      setQuotes(response.data);
    } catch (error) {
      console.error('Error fetching quotes:', error);
    }
  };

  const handleTagInput = async (value) => {
    try {
      const response = await axios.get(`${API_BASE_URL}/tags`);
      const filteredTags = response.data
        .filter(tag => tag.name.toLowerCase().includes(value.toLowerCase()))
        .map(tag => tag.name);
      setTagSuggestions(filteredTags);
    } catch (error) {
      console.error('Error fetching tags:', error);
    }
  };

  const handleAddQuote = async () => {
    try {
      const response = await axios.post(API_BASE_URL, {
        content: newQuote.content,
        author: newQuote.author
      });
      
      if (newQuote.tags) {
        const tags = newQuote.tags.split(',').map(tag => tag.trim());
        for (const tag of tags) {
          await axios.post(`${API_BASE_URL}/${response.data.quoteId}/tags`, { name: tag });
        }
      }
      
      setShowAddModal(false);
      setNewQuote({ content: '', author: '', tags: '' });
      fetchQuotes();
    } catch (error) {
      console.error('Error adding quote:', error);
    }
  };

  const handleEditQuote = async () => {
    try {
      await axios.put(`${API_BASE_URL}/${currentQuote.quoteId}`, currentQuote);
      setShowEditModal(false);
      setCurrentQuote(null);
      fetchQuotes();
    } catch (error) {
      console.error('Error updating quote:', error);
    }
  };

  const handleLike = async (quoteId) => {
    try {
      await axios.post(`${API_BASE_URL}/${quoteId}/like`);
      fetchQuotes();
    } catch (error) {
      console.error('Error liking quote:', error);
    }
  };

  return (
    <div className="container">
      <h1>Quotes Application</h1>
      
      <div className="controls">
        <button onClick={() => setShowAddModal(true)}>Add New Quote</button>
        <input
          type="text"
          placeholder="Filter by tag..."
          value={filterTag}
          onChange={(e) => setFilterTag(e.target.value)}
        />
        <button onClick={() => setShowTopQuotes(!showTopQuotes)}>
          {showTopQuotes ? 'Show All Quotes' : 'Show Top 10'}
        </button>
      </div>

      <div className="quotes-grid">
        {quotes.map(quote => (
          <div key={quote.quoteId} className="quote-card">
            <p className="quote-content">{quote.content}</p>
            <p className="quote-author">- {quote.author}</p>
            <div className="quote-tags">
              {quote.tagAssignments?.map(ta => (
                <span key={ta.tagId} className="tag">{ta.tag.name}</span>
              ))}
            </div>
            <div className="quote-actions">
              <button onClick={() => handleLike(quote.quoteId)}>
                ❤️ {quote.likes}
              </button>
              <button onClick={() => {
                setCurrentQuote(quote);
                setShowEditModal(true);
              }}>Edit</button>
            </div>
          </div>
        ))}
      </div>

      {!showTopQuotes && !filterTag && (
        <div className="pagination">
          <button
            disabled={currentPage === 1}
            onClick={() => setCurrentPage(p => p - 1)}
          >
            Previous
          </button>
          <span>Page {currentPage}</span>
          <button onClick={() => setCurrentPage(p => p + 1)}>Next</button>
        </div>
      )}

      {showAddModal && (
        <div className="modal">
          <div className="modal-content">
            <h2>Add New Quote</h2>
            <input
              type="text"
              placeholder="Quote content..."
              value={newQuote.content}
              onChange={(e) => setNewQuote({...newQuote, content: e.target.value})}
            />
            <input
              type="text"
              placeholder="Author..."
              value={newQuote.author}
              onChange={(e) => setNewQuote({...newQuote, author: e.target.value})}
            />
            <input
              type="text"
              placeholder="Tags (comma-separated)..."
              value={newQuote.tags}
              onChange={(e) => {
                setNewQuote({...newQuote, tags: e.target.value});
                handleTagInput(e.target.value);
              }}
            />
            {tagSuggestions.length > 0 && (
              <div className="tag-suggestions">
                {tagSuggestions.map(tag => (
                  <div
                    key={tag}
                    onClick={() => {
                      setNewQuote({...newQuote, tags: tag});
                      setTagSuggestions([]);
                    }}
                  >
                    {tag}
                  </div>
                ))}
              </div>
            )}
            <div className="modal-actions">
              <button onClick={handleAddQuote}>Add</button>
              <button onClick={() => {
                setShowAddModal(false);
                setNewQuote({ content: '', author: '', tags: '' });
              }}>Cancel</button>
            </div>
          </div>
        </div>
      )}

      {showEditModal && currentQuote && (
        <div className="modal">
          <div className="modal-content">
            <h2>Edit Quote</h2>
            <input
              type="text"
              value={currentQuote.content}
              onChange={(e) => setCurrentQuote({...currentQuote, content: e.target.value})}
            />
            <input
              type="text"
              value={currentQuote.author}
              onChange={(e) => setCurrentQuote({...currentQuote, author: e.target.value})}
            />
            <div className="modal-actions">
              <button onClick={handleEditQuote}>Save</button>
              <button onClick={() => {
                setShowEditModal(false);
                setCurrentQuote(null);
              }}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;